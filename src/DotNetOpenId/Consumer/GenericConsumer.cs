namespace DotNetOpenId.Consumer
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Net;
	using System.Security.Cryptography;
	using System.Text;
	using Org.Mentalis.Security.Cryptography;
	using DotNetOpenId;
	using System.Web;
	using System.IO;
	using System.Diagnostics;
	using System.Globalization;

	internal class GenericConsumer
	{
		static TimeSpan minimumUsefulAssociationLifetime {
			get { return OpenIdConsumer.MaximumUserAgentAuthenticationTime; }
		}

		IConsumerApplicationStore store;

		public GenericConsumer(IConsumerApplicationStore store)
		{
			this.store = store;
		}

		public AuthenticationRequest Begin(ServiceEndpoint service_endpoint,
			TrustRoot trustRootUrl, Uri returnToUrl)
		{
			string token = new Token(service_endpoint).Serialize(store);

			Association assoc = this.getAssociation(service_endpoint.ServerUrl);

			AuthenticationRequest request = new AuthenticationRequest(token, assoc, service_endpoint,
				trustRootUrl, returnToUrl);

			return request;
		}

		public AuthenticationResponse Complete(IDictionary<string, string> query)
		{
			string mode;
			if (!query.TryGetValue(QueryStringArgs.openid.mode, out mode))
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture, 
					Strings.MissingOpenIdQueryParameter, QueryStringArgs.openid.mode));

			string tokenString;
			if (!query.TryGetValue(Token.TokenKey, out tokenString))
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.MissingInternalQueryParameter, Token.TokenKey));
			Token token = Token.Deserialize(tokenString, store);

			switch (mode) {
				case QueryStringArgs.Modes.cancel:
					return new AuthenticationResponse(AuthenticationStatus.Canceled, token.IdentityUrl, query);
				case QueryStringArgs.Modes.error:
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						"The provider returned an error: {0}", query[QueryStringArgs.openid.error],
						token.IdentityUrl));
				case QueryStringArgs.Modes.id_res:
					return doIdRes(query, token);
				default:
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.InvalidOpenIdQueryParameterValue,
						QueryStringArgs.openid.mode, mode), token.IdentityUrl);
			}
		}

		/// <summary>
		/// Performs a dumb-mode authentication verification by making an extra
		/// request to the provider after the user agent was redirected back
		/// to the consumer site with an authenticated status.
		/// </summary>
		/// <returns>Whether the authentication is valid.</returns>
		bool checkAuth(IDictionary<string, string> query, Uri serverUrl)
		{
			IDictionary<string, string> request = createCheckAuthRequest(query);

			if (request == null)
				return false;

			var response = makeKVPost(request, serverUrl);

			if (response == null)
				return false;

			return ProcessCheckAuthResponse(response, serverUrl);
		}

		public void detectAlteredArguments(AuthenticationResponse response, 
			IDictionary<string, string> query, params string[] argumentNames) {
			
			NameValueCollection return_to = HttpUtility.ParseQueryString(response.ReturnTo.Query);

			foreach (string arg in argumentNames) {
				string queryArg;
				query.TryGetValue(arg, out queryArg);
				if (queryArg != return_to[arg])
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.ReturnToArgDifferentFromQueryArg, arg, return_to[arg], query[arg]));
			}
		}

		static IDictionary<string, string> makeKVPost(IDictionary<string, string> args, Uri serverUrl) {
			byte[] body = ProtocolMessages.Http.GetBytes(args);

			try {
				FetchResponse resp = Fetcher.Request(serverUrl, body);
				if ((int)resp.Code >= 200 && (int)resp.Code < 300) {
					return ProtocolMessages.KeyValueForm.GetDictionary(resp.Data);
				} else {
					if (TraceUtil.Switch.TraceError) {
						Trace.TraceError("Bad request code returned from remote server: {0}.", resp.Code);
					}
					return null;
				}
			} catch (ArgumentException e) {
				if (TraceUtil.Switch.TraceWarning)
					Trace.TraceWarning("Failure decoding Key-Value Form response from provider.");
				return null;
			} catch (WebException e) {
				Trace.TraceError("Failure while connecting to remote server: {0}.", e.Message);
				return null;
			}
		}

		string getRequiredField(IDictionary<string, string> query, string key) {
			string val;
			if (!query.TryGetValue(key, out val))
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.MissingOpenIdQueryParameter, key));

			return val;
		}

		AuthenticationResponse doIdRes(IDictionary<string, string> query, Token token)
		{
			string user_setup_url;
			if (query.TryGetValue(QueryStringArgs.openid.user_setup_url, out user_setup_url))
				return new AuthenticationResponse(AuthenticationStatus.SetupRequired, token.IdentityUrl, query);

			string assoc_handle = getRequiredField(query, QueryStringArgs.openid.assoc_handle);

			// TODO: I'm pretty sure this check doesn't accomplish anything. (ALA)
			if (token.ServerId.AbsoluteUri != token.ServerId.ToString())
				throw new OpenIdException("Provider ID (delegate) mismatch", token.IdentityUrl);

			Association assoc = store.GetAssociation(token.ServerUrl, assoc_handle);
			AuthenticationResponse response;

			if (assoc == null) {
				// It's not an association we know about.  Dumb mode is our
				// only possible path for recovery.
				if (!checkAuth(query, token.ServerUrl))
					throw new OpenIdException("check_authentication failed", token.IdentityUrl);

				response = new AuthenticationResponse(AuthenticationStatus.Authenticated, token.IdentityUrl, query);
			} else {
				if (assoc.IsExpired)
					throw new OpenIdException(String.Format(CultureInfo.CurrentUICulture,
						"Association with {0} expired", token.ServerUrl), token.IdentityUrl);

				verifySignature(query, assoc);

				response = new AuthenticationResponse(AuthenticationStatus.Authenticated, token.IdentityUrl, query);
			}

			// Just a little extra something to make sure that what's signed in return_to
			// and doubled in the actual returned arguments is the same.
			detectAlteredArguments(response, query, Token.TokenKey);

			return response;
		}

		/// <summary>
		/// Verifies that a query is signed and that the signed fields have not been tampered with.
		/// </summary>
		/// <exception cref="OpenIdException">Thrown when the signature is missing or the query has been tampered with.</exception>
		void verifySignature(IDictionary<string, string> query, Association assoc) {
			string sig = getRequiredField(query, QueryStringArgs.openid.sig);
			string signed = getRequiredField(query, QueryStringArgs.openid.signed);
			string[] signed_array = signed.Split(',');

			string v_sig = CryptUtil.ToBase64String(assoc.Sign(query, signed_array, QueryStringArgs.openid.Prefix));

			if (v_sig != sig)
				throw new OpenIdException(Strings.InvalidSignature);
		}

		static AssociationRequest createAssociationRequest(Uri serverUrl)
		{
			var args = new Dictionary<string, string>();

			args.Add(QueryStringArgs.openid.mode, QueryStringArgs.Modes.associate);
			args.Add(QueryStringArgs.openid.assoc_type, QueryStringArgs.HMAC_SHA1);

			DiffieHellman dh = null;

			if (serverUrl.Scheme != Uri.UriSchemeHttps)
			{
				// Initiate Diffie-Hellman Exchange
				dh = CryptUtil.CreateDiffieHellman();

				byte[] dhPublic = dh.CreateKeyExchange();
				string cpub = CryptUtil.UnsignedToBase64(dhPublic);

				args.Add(QueryStringArgs.openid.session_type, QueryStringArgs.DH_SHA1);
				args.Add(QueryStringArgs.openid.dh_consumer_public, cpub);

				DHParameters dhps = dh.ExportParameters(true);

				if (dhps.P != CryptUtil.DEFAULT_MOD || dhps.G != CryptUtil.DEFAULT_GEN)
				{
					args.Add(QueryStringArgs.openid.dh_modulus, CryptUtil.UnsignedToBase64(dhps.P));
					args.Add(QueryStringArgs.openid.dh_gen, CryptUtil.UnsignedToBase64(dhps.G));
				}
			}

			return new AssociationRequest(dh, args);
		}

		static IDictionary<string, string> createCheckAuthRequest(IDictionary<string, string> query)
		{
			string signed = query[QueryStringArgs.openid.signed];

			if (signed == null)
				// #XXX: oidutil.log('No signature present; checkAuth aborted')
				return null;

			// Arguments that are always passed to the server and not
			// included in the signature.
			string[] whitelist = new string[] { QueryStringArgs.openidnp.assoc_handle, QueryStringArgs.openidnp.sig, QueryStringArgs.openidnp.signed, QueryStringArgs.openidnp.invalidate_handle };
			string[] splitted = signed.Split(',');

			// combine the previous 2 arrays (whitelist + splitted) into a new array: signed_array
			string[] signed_array = new string[whitelist.Length + splitted.Length];
			Array.Copy(whitelist, signed_array, whitelist.Length);
			Array.Copy(splitted, 0, signed_array, whitelist.Length, splitted.Length);

			var check_args = new Dictionary<string, string>();

			foreach (string key in query.Keys)
			{
				if (key.StartsWith(QueryStringArgs.openid.Prefix, StringComparison.OrdinalIgnoreCase) 
					&& Array.IndexOf(signed_array, key.Substring(QueryStringArgs.openid.Prefix.Length)) > -1)
					check_args[key] = query[key];
			}
			check_args[QueryStringArgs.openid.mode] = QueryStringArgs.Modes.check_authentication;

			return check_args;
		}

		Association getAssociation(Uri serverUrl)
		{
			Association assoc = store.GetAssociation(serverUrl);

			if (assoc == null || assoc.SecondsTillExpiration < minimumUsefulAssociationLifetime.TotalSeconds)
			{
				AssociationRequest req = createAssociationRequest(serverUrl);

				var response = makeKVPost(req.Args, serverUrl);

				if (response == null)
					assoc = null;
				else
					assoc = ParseAssociation(response, req.DH, serverUrl);
			}

			return assoc;
		}

		internal Association ParseAssociation(IDictionary<string, string> results, DiffieHellman dh, Uri server_url) {
			Converter<string, string> getParameter = delegate(string key) {
				string val;
				if (!results.TryGetValue(key, out val) || string.IsNullOrEmpty(val))
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture, Strings.MissingOpenIdQueryParameter, key));
				return val;
			};

			Converter<string, byte[]> getDecoded = delegate(string key) {
				try {
					return Convert.FromBase64String(getParameter(key));
				} catch (FormatException ex) {
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.ExpectedBase64OpenIdQueryParameter, key), null, ex);
				}
			};

			try {
				Association assoc;
				string assoc_type = getParameter(QueryStringArgs.openidnp.assoc_type);
				switch (assoc_type) {
					case QueryStringArgs.HMAC_SHA1:
						byte[] secret;

						string session_type;
						if (!results.TryGetValue(QueryStringArgs.openidnp.session_type, out session_type)) {
							secret = getDecoded(QueryStringArgs.mac_key);
						} else if (QueryStringArgs.DH_SHA1.Equals(session_type, StringComparison.Ordinal)) {
							byte[] dh_server_public = getDecoded(QueryStringArgs.openidnp.dh_server_public);
							byte[] enc_mac_key = getDecoded(QueryStringArgs.enc_mac_key);
							secret = CryptUtil.SHA1XorSecret(dh, dh_server_public, enc_mac_key);
						} else // # XXX: log this
							return null;

						string assocHandle = getParameter(QueryStringArgs.openidnp.assoc_handle);
						TimeSpan expiresIn = new TimeSpan(0, 0, Convert.ToInt32(getParameter(QueryStringArgs.openidnp.expires_in), CultureInfo.CurrentUICulture));

						assoc = new HmacSha1Association(assocHandle, secret, expiresIn);
						break;
					default:
						Trace.TraceError("Unrecognized assoc_type '{0}'.", assoc_type);
						assoc = null;
						break;
				}

				store.StoreAssociation(server_url, assoc);

				return assoc;
			} catch (OpenIdException ex) {
				if (TraceUtil.Switch.TraceError) {
					Trace.TraceError(ex.ToString());
				}
				return null;
			}
		}

		private bool ProcessCheckAuthResponse(IDictionary<string, string> response, Uri server_url)
		{
			string is_valid;
			response.TryGetValue(QueryStringArgs.openidnp.is_valid, out is_valid);

			if (is_valid == QueryStringArgs.IsValid.True)
			{
				string invalidate_handle = response[QueryStringArgs.openidnp.invalidate_handle];
				if (invalidate_handle != null)
					store.RemoveAssociation(server_url, invalidate_handle);

				return true;
			}

			// XXX: Log this
			return false;
		}

		class AssociationRequest
		{
			public AssociationRequest(DiffieHellman dh, IDictionary<string, string> nvc)
			{
				this.DH = dh;
				this.Args = nvc;
			}

			public readonly DiffieHellman DH;
			public readonly IDictionary<string, string> Args;
		}

	}
}
