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
		Fetcher fetcher;

		public GenericConsumer(IConsumerAssociationStore store, Fetcher fetcher)
		{
			this.store = store;
			this.fetcher = fetcher;
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
				mode = "<no mode specified>";

			string tokenString;
			if (!query.TryGetValue(Token.TokenKey, out tokenString))
				throw new FailureException(null, "No token found.");
			Token token = Token.Deserialize(tokenString, store.AuthKey);

			if (mode == QueryStringArgs.Modes.cancel)
				throw new CancelException(token.IdentityUrl);

			if (mode == QueryStringArgs.Modes.error)
			{
				string error = query[QueryStringArgs.openid.error];

				throw new FailureException(token.IdentityUrl, error);
			}

			if (mode == QueryStringArgs.Modes.id_res)
			{
				if (token.IdentityUrl == null)
					throw new FailureException(token.IdentityUrl, "No session state found");

				ConsumerResponse response = DoIdRes(query, token);

				checkNonce(response, query[QueryStringArgs.nonce]);

				return response;
			}

			throw new FailureException(token.IdentityUrl, "Invalid openid.mode: " + mode);
		}

		private bool CheckAuth(IDictionary<string, string> query, Uri server_url)
		{
			IDictionary<string, string> request = CreateCheckAuthRequest(query);

			if (request == null)
				return false;

			var response = MakeKVPost(request, server_url);

			if (response == null)
				return false;

			return ProcessCheckAuthResponse(response, server_url);
		}

		void checkNonce(ConsumerResponse response, string nonce)
		{
			var nvc = HttpUtility.ParseQueryString(response.ReturnTo.Query);

			string value = nvc[QueryStringArgs.nonce];
			if (String.IsNullOrEmpty(value))
				throw new FailureException(response.IdentityUrl,
							   "Nonce missing from return_to: " +
							   response.ReturnTo.AbsoluteUri);

			if (value != nonce)
				throw new FailureException(response.IdentityUrl, "Nonce mismatch");
		}

		IDictionary<string, string> MakeKVPost(IDictionary<string, string> args, Uri server_url) {
			byte[] body = ASCIIEncoding.ASCII.GetBytes(UriUtil.CreateQueryString(args));

			try {
				FetchResponse resp = fetcher.Post(server_url, body);

				return DictionarySerializer.Deserialize(resp.Data, resp.Length);
			} catch (FetchException e) {
				if (e.response.Code == HttpStatusCode.BadRequest) {
					Trace.TraceError("Bad request code returned from post attempt.");
				} else {
					Trace.TraceError("Some FetchException caught during post attempt: {0}", e);
				}
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
				if (!CheckAuth(query, token.ServerUrl))
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
					throw new MissingParameterException(key);
				return val;
			};

			Converter<string, byte[]> getDecoded = delegate(string key) {
				try {
					return Convert.FromBase64String(getParameter(key));
				} catch (FormatException) {
					throw new MissingParameterException("Query argument is not base64: " + key);
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
			} catch (MissingParameterException ex) {
				Trace.TraceError("Missing parameter: {0}", ex.Message);
				return null;
			}
		}

		public class MissingParameterException : ApplicationException
		{
			public MissingParameterException(string key)
				: base("Query missing key: " + key)
			{
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
