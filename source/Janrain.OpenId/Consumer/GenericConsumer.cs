namespace Janrain.OpenId.Consumer
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Net;
	using System.Security.Cryptography;
	using System.Text;
	using Org.Mentalis.Security.Cryptography;
	using Janrain.OpenId;
	using Janrain.OpenId.Store;
	using System.Web;
	using System.IO;

	public class GenericConsumer
	{
		private static uint TOKEN_LIFETIME = 120;

		private IAssociationStore store;
		private Fetcher fetcher;

		public GenericConsumer(IAssociationStore store, Fetcher fetcher)
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

		public ConsumerResponse Complete(NameValueCollection query, string tokenString)
		{
			string mode = query[QueryStringArgs.openid.mode];
			if (mode == null)
				mode = "<no mode specified>";

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

				ConsumerResponse response = DoIdRes(query, token.IdentityUrl, token.ServerId, token.ServerUrl);

				CheckNonce(response, query[QueryStringArgs.nonce]);

				return response;
			}
			
			throw new FailureException(token.IdentityUrl, "Invalid openid.mode: " + mode);
		}

		private bool CheckAuth(NameValueCollection query, Uri server_url)
		{
			NameValueCollection request = CreateCheckAuthRequest(query);

			if (request == null)
				return false;

			IDictionary response = MakeKVPost(request, server_url);

			if (response == null)
				return false;

			return ProcessCheckAuthResponse(response, server_url);
		}

		private void CheckNonce(ConsumerResponse response, string nonce)
		{
			NameValueCollection nvc = HttpUtility.ParseQueryString(response.ReturnTo.Query);

			string value = nvc[QueryStringArgs.nonce];
			if (String.IsNullOrEmpty(value))
				throw new FailureException(response.IdentityUrl,
							   "Nonce missing from return_to: " +
							   response.ReturnTo.AbsoluteUri);

			if (value != nonce)
				throw new FailureException(response.IdentityUrl, "Nonce mismatch");
		}

		private IDictionary MakeKVPost(NameValueCollection args, Uri server_url)
		{
			byte[] body = ASCIIEncoding.ASCII.GetBytes(UriUtil.CreateQueryString(args));

			try
			{
				FetchResponse resp = this.fetcher.Post(server_url, body);

				return KVUtil.KVToDict(resp.data);
			}
			catch // (FetchException e)
			{
				//if (e.response == null)
				//    return null;
				//else (if e.response.code == HttpStatusCode.BadRequest)
				//    # XXX: log this
				//    pass
				//else:
				//    # XXX: log this
				//    pass

				return null;
			}
		}

		private ConsumerResponse DoIdRes(NameValueCollection query, Uri consumer_id, Uri server_id, Uri server_url)
		{
			Converter<string, string> getRequired = delegate(string key)
				{
					string val = query[key];
					if (val == null)
						throw new FailureException(consumer_id, "Missing required field: " + key);

					return val;
				};

			string user_setup_url = query[QueryStringArgs.openid.user_setup_url];
			if (user_setup_url != null)
				throw new SetupNeededException(consumer_id, new Uri(user_setup_url));

			string return_to = getRequired(QueryStringArgs.openid.return_to);
			string server_id2 = getRequired(QueryStringArgs.openid.identity);
			string assoc_handle = getRequired(QueryStringArgs.openid.assoc_handle);

			if (server_id.AbsoluteUri != server_id.ToString())
				throw new FailureException(consumer_id, "Server ID (delegate) mismatch");

			Association assoc = this.store.GetAssociation(server_url, assoc_handle);

			if (assoc == null)
			{
				// It's not an association we know about.  Dumb mode is our
				// only possible path for recovery.
				if (!CheckAuth(query, server_url))
					throw new FailureException(consumer_id, "check_authentication failed");

				return new ConsumerResponse(consumer_id, query, query[QueryStringArgs.openid.signed]);
			}

			if (assoc.ExpiresIn <= 0)
			{
				throw new FailureException(consumer_id, String.Format("Association with {0} expired", server_url));
			}

			// Check the signature
			string sig = getRequired(QueryStringArgs.openid.sig);
			string signed = getRequired(QueryStringArgs.openid.signed);
			string[] signed_array = signed.Split(',');

			string v_sig = assoc.SignDict(signed_array, query, QueryStringArgs.openid.Prefix);

			if (v_sig != sig)
				throw new FailureException(consumer_id, "Bad signature");

			return new ConsumerResponse(consumer_id, query, signed);
		}

		private static AssociationRequest CreateAssociationRequest(Uri server_url)
		{
			NameValueCollection args = new NameValueCollection();

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

		private NameValueCollection CreateCheckAuthRequest(NameValueCollection query)
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

			NameValueCollection check_args = new NameValueCollection();

			foreach (string key in query.AllKeys)
			{
				if (key.StartsWith(QueryStringArgs.openid.Prefix) 
					&& Array.IndexOf(signed_array, key.Substring(QueryStringArgs.openid.Prefix.Length)) > -1)
					check_args.Add(key, query[key]);

				check_args[QueryStringArgs.openid.mode] = QueryStringArgs.Modes.check_authentication;
			}

			return check_args;
		}

		private Association GetAssociation(Uri server_url)
		{
			if (this.store.IsDumb)
				return null;

			Association assoc = this.store.GetAssociation(server_url);

			if (assoc == null || assoc.ExpiresIn < TOKEN_LIFETIME)
			{
				AssociationRequest req = CreateAssociationRequest(server_url);

				IDictionary response = MakeKVPost(req.Args, server_url);

				if (response == null)
					assoc = null;
				else
					assoc = ParseAssociation(response, req.DH, server_url);
			}

			return assoc;
		}

		protected HMACSHA1Association ParseAssociation(IDictionary results, DiffieHellman dh, Uri server_url)
		{
			Converter<string, string> getParameter = delegate(string key)
			{
				string val = (string)results[key];
				if (val == null)
					throw new MissingParameterException("Query args missing key: " + key);

				return val;
			};

			Converter<string, byte[]> getDecoded = delegate(string key)
				{
					try
					{
						return Convert.FromBase64String(getParameter(key));
					}
					catch (FormatException)
					{
						throw new MissingParameterException("Query argument is not base64: " + key);
					}
				};

			try
			{
				if (getParameter(QueryStringArgs.openidnp.assoc_type) != QueryStringArgs.HMAC_SHA1)
					// XXX: log this
					return null;

				byte[] secret;

				string session_type = (string)results[QueryStringArgs.openidnp.session_type];

				if (session_type == null)
					secret = getDecoded(QueryStringArgs.mac_key);
				else if (session_type == QueryStringArgs.DH_SHA1)
				{
					byte[] dh_server_public = getDecoded(QueryStringArgs.openidnp.dh_server_public);
					byte[] enc_mac_key = getDecoded(QueryStringArgs.enc_mac_key);
					secret = CryptUtil.SHA1XorSecret(dh, dh_server_public, enc_mac_key);
				}
				else // # XXX: log this
					return null;

				string assocHandle = getParameter(QueryStringArgs.openidnp.assoc_handle);
				TimeSpan expiresIn = new TimeSpan(0, 0, Convert.ToInt32(getParameter(QueryStringArgs.openidnp.expires_in)));

				HMACSHA1Association assoc = new HMACSHA1Association(assocHandle, secret, expiresIn);
				this.store.StoreAssociation(server_url, assoc);

				return assoc;
			}
			catch (MissingParameterException)
			{

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
		private bool ProcessCheckAuthResponse(IDictionary response, Uri server_url)
		{
			string is_valid = (string)response[QueryStringArgs.openidnp.is_valid];

			if (is_valid == "true")
			{
				string invalidate_handle = (string)response[QueryStringArgs.openidnp.invalidate_handle];
				if (invalidate_handle != null)
					this.store.RemoveAssociation(server_url, invalidate_handle);

				return true;
			}

			// XXX: Log this
			return false;
		}

		class AssociationRequest
		{
			public AssociationRequest(DiffieHellman dh, NameValueCollection nvc)
			{
				this.DH = dh;
				this.Args = nvc;
			}

			public readonly DiffieHellman DH;
			public readonly NameValueCollection Args;
		}

	}
}
