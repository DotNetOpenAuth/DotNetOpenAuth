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
	using DotNetOpenId.Store;
	using System.Web;
	using System.IO;
	using System.Diagnostics;
	using IConsumerAssociationStore = DotNetOpenId.Store.IAssociationStore<System.Uri>;

	internal class GenericConsumer
	{
		static readonly TimeSpan minimumUsefulAssociationLifetime = TimeSpan.FromSeconds(120);

		IConsumerAssociationStore store;

		public GenericConsumer(IConsumerAssociationStore store)
		{
			this.store = store;
		}

		public AuthRequest Begin(ServiceEndpoint service_endpoint)
		{
			string nonce = CryptUtil.CreateNonce();
			string token = new Token(service_endpoint).Serialize(store.AuthKey);

			Association assoc = this.GetAssociation(service_endpoint.ServerUrl);

			AuthRequest request = new AuthRequest(token, assoc, service_endpoint);
			request.ReturnToArgs.Add(QueryStringArgs.nonce, nonce);

			return request;
		}

		public ConsumerResponse Complete(IDictionary<string, string> query)
		{
			string mode;
			if (!query.TryGetValue(QueryStringArgs.openid.mode, out mode))
				throw new ProtocolException(string.Format(Strings.MissingOpenIdQueryParameter, QueryStringArgs.openid.mode));

			string tokenString;
			if (!query.TryGetValue(Token.TokenKey, out tokenString))
				throw new ProtocolException(string.Format(Strings.MissingInternalQueryParameter, Token.TokenKey));
			Token token = Token.Deserialize(tokenString, store.AuthKey);

			switch (mode) {
				case QueryStringArgs.Modes.cancel:
					throw new CancelException(token.IdentityUrl);
				case QueryStringArgs.Modes.error:
					throw new FailureException(token.IdentityUrl, query[QueryStringArgs.openid.error]);
				case QueryStringArgs.Modes.id_res:
					ConsumerResponse response = DoIdRes(query, token);
					checkNonce(response, query[QueryStringArgs.nonce]);
					return response;
				default:
					throw new ProtocolException(string.Format(Strings.InvalidOpenIdQueryParameterValue,
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
			IDictionary<string, string> request = CreateCheckAuthRequest(query);

			if (request == null)
				return false;

			var response = MakeKVPost(request, serverUrl);

			if (response == null)
				return false;

			return ProcessCheckAuthResponse(response, serverUrl);
		}

		/// <summary>
		/// Checks that a given nonce is valid, and that it has only been used once
		/// to protect against replay attacks.
		/// </summary>
		/// <remarks>
		/// TODO: replay attacks are not currently guarded against.
		/// </remarks>
		void checkNonce(ConsumerResponse response, string nonce)
		{
			var nvc = HttpUtility.ParseQueryString(response.ReturnTo.Query);

			string returnToNonce = nvc[QueryStringArgs.nonce];
			if (String.IsNullOrEmpty(returnToNonce))
				throw new ProtocolException(string.Format(Strings.MissingReturnToQueryParameter,
					QueryStringArgs.nonce, response.ReturnTo.Query), response.IdentityUrl);

			if (returnToNonce != nonce)
				throw new ProtocolException(Strings.NonceMismatch, response.IdentityUrl);
		}

		IDictionary<string, string> MakeKVPost(IDictionary<string, string> args, Uri server_url) {
			byte[] body = ASCIIEncoding.ASCII.GetBytes(UriUtil.CreateQueryString(args));

			try {
				FetchResponse resp = Fetcher.Request(server_url, body);
				if ((int)resp.Code > 200 && (int)resp.Code < 300) {
					return DictionarySerializer.Deserialize(resp.Data, resp.Length);
				} else {
					if (TraceUtil.Switch.TraceError) {
						Trace.TraceError("Bad request code returned from remote server: {0}.", resp.Code);
					}
					return null;
				}
			} catch (WebException e) {
				Trace.TraceError("Failure while connecting to remote server: {0}.", e.Message);
				return null;
			}
		}

		private ConsumerResponse DoIdRes(IDictionary<string, string> query, Token token)
		{
			Converter<string, string> getRequired = delegate(string key)
				{
					string val;
					if (!query.TryGetValue(key, out val))
						throw new FailureException(token.IdentityUrl, "Missing required field: " + key);

					return val;
				};

			string user_setup_url;
			if (query.TryGetValue(QueryStringArgs.openid.user_setup_url, out user_setup_url))
				throw new SetupNeededException(token.IdentityUrl, new Uri(user_setup_url));

			string return_to = getRequired(QueryStringArgs.openid.return_to);
			string server_id2 = getRequired(QueryStringArgs.openid.identity);
			string assoc_handle = getRequired(QueryStringArgs.openid.assoc_handle);

			if (token.ServerId.AbsoluteUri != token.ServerId.ToString())
				throw new FailureException(token.IdentityUrl, "Provider ID (delegate) mismatch");

			Association assoc = this.store.GetAssociation(token.ServerUrl, assoc_handle);

			if (assoc == null)
			{
				// It's not an association we know about.  Dumb mode is our
				// only possible path for recovery.
				if (!checkAuth(query, token.ServerUrl))
					throw new FailureException(token.IdentityUrl, "check_authentication failed");

				return new ConsumerResponse(token.IdentityUrl, query, query[QueryStringArgs.openid.signed]);
			}

			if (assoc.IsExpired)
			{
				throw new FailureException(token.IdentityUrl, String.Format("Association with {0} expired", token.ServerUrl));
			}

			// Check the signature
			string sig = getRequired(QueryStringArgs.openid.sig);
			string signed = getRequired(QueryStringArgs.openid.signed);
			string[] signed_array = signed.Split(',');

			string v_sig = CryptUtil.ToBase64String(assoc.Sign(query, signed_array, QueryStringArgs.openid.Prefix));

			if (v_sig != sig)
				throw new FailureException(token.IdentityUrl, "Bad signature");

			return new ConsumerResponse(token.IdentityUrl, query, signed);
		}

		private static AssociationRequest CreateAssociationRequest(Uri server_url)
		{
			var args = new Dictionary<string, string>();

			args.Add(QueryStringArgs.openid.mode, QueryStringArgs.Modes.associate);
			args.Add(QueryStringArgs.openid.assoc_type, QueryStringArgs.HMAC_SHA1);

			DiffieHellman dh = null;

			if (server_url.Scheme != Uri.UriSchemeHttps)
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

		private IDictionary<string, string> CreateCheckAuthRequest(IDictionary<string, string> query)
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
				if (key.StartsWith(QueryStringArgs.openid.Prefix) 
					&& Array.IndexOf(signed_array, key.Substring(QueryStringArgs.openid.Prefix.Length)) > -1)
					check_args[key] = query[key];
			}
			check_args[QueryStringArgs.openid.mode] = QueryStringArgs.Modes.check_authentication;

			return check_args;
		}

		private Association GetAssociation(Uri server_url)
		{
			Association assoc = store.GetAssociation(server_url);

			if (assoc == null || assoc.SecondsTillExpiration < minimumUsefulAssociationLifetime.TotalSeconds)
			{
				AssociationRequest req = CreateAssociationRequest(server_url);

				var response = MakeKVPost(req.Args, server_url);

				if (response == null)
					assoc = null;
				else
					assoc = ParseAssociation(response, req.DH, server_url);
			}

			return assoc;
		}

		internal Association ParseAssociation(IDictionary<string, string> results, DiffieHellman dh, Uri server_url) {
			Converter<string, string> getParameter = delegate(string key) {
				string val;
				if (!results.TryGetValue(key, out val) || string.IsNullOrEmpty(val))
					throw new ProtocolException(string.Format(Strings.MissingOpenIdQueryParameter, key));
				return val;
			};

			Converter<string, byte[]> getDecoded = delegate(string key) {
				try {
					return Convert.FromBase64String(getParameter(key));
				} catch (FormatException ex) {
					throw new ProtocolException(string.Format(
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
						TimeSpan expiresIn = new TimeSpan(0, 0, Convert.ToInt32(getParameter(QueryStringArgs.openidnp.expires_in)));

						assoc = new HmacSha1Association(assocHandle, secret, expiresIn);
						break;
					default:
						Trace.TraceError("Unrecognized assoc_type '{0}'.", assoc_type);
						assoc = null;
						break;
				}

				store.StoreAssociation(server_url, assoc);

				return assoc;
			} catch (ProtocolException ex) {
				if (TraceUtil.Switch.TraceError) {
					Trace.TraceError(ex.ToString());
				}
				return null;
			}
		}

		private bool ProcessCheckAuthResponse(IDictionary<string, string> response, Uri server_url)
		{
			string is_valid = response[QueryStringArgs.openidnp.is_valid];

			if (is_valid == "true")
			{
				string invalidate_handle = response[QueryStringArgs.openidnp.invalidate_handle];
				if (invalidate_handle != null)
					this.store.RemoveAssociation(server_url, invalidate_handle);

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
