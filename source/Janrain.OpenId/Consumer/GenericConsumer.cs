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

		private const string DH_SHA1 = "DH-SHA1";
		private const string HMAC_SHA1 = "HMAC-SHA1";

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
			string token = GenToken(service_endpoint);

			Association assoc = this.GetAssociation(service_endpoint.ServerUrl);

			AuthRequest request = new AuthRequest(token, assoc, service_endpoint);
			request.ReturnToArgs.Add("nonce", nonce);

			return request;
		}

		public ConsumerResponse Complete(NameValueCollection query, string token)
		{
			string mode = query["openid.mode"];
			if (mode == null)
				mode = "<no mode specified>";

			Uri identity_url = null;
			Uri server_id = null;
			Uri server_url = null;

			IList pieces = SplitToken(token);

			if (pieces != null)
			{
				identity_url = (Uri)pieces[0];
				server_id = (Uri)pieces[1];
				server_url = (Uri)pieces[2];
			}

			if (mode == "cancel")
				throw new CancelException(identity_url);
			
			if (mode == "error")
			{
				string error = query["openid.error"];

				throw new FailureException(identity_url, error);
			}

			if (mode == "id_res")
			{
				if (identity_url == null)
					throw new FailureException(identity_url, "No session state found");

				ConsumerResponse response = DoIdRes(query, identity_url, server_id, server_url);

				CheckNonce(response, query["nonce"]);

				return response;
			}
			
			throw new FailureException(identity_url, "Invalid openid.mode: " + mode);
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

			string value = nvc["nonce"];
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
					string val = query["openid." + key];
					if (val == null)
						throw new FailureException(consumer_id, "Missing required field: " + key);

					return val;
				};

			string user_setup_url = query["openid.user_setup_url"];
			if (user_setup_url != null)
				throw new SetupNeededException(consumer_id, new Uri(user_setup_url));

			string return_to = getRequired("return_to");
			string server_id2 = getRequired("identity");
			string assoc_handle = getRequired("assoc_handle");

			if (server_id.AbsoluteUri != server_id.ToString())
				throw new FailureException(consumer_id, "Server ID (delegate) mismatch");

			Association assoc = this.store.GetAssociation(server_url, assoc_handle);

			if (assoc == null)
			{
				// It's not an association we know about.  Dumb mode is our
				// only possible path for recovery.
				if (!CheckAuth(query, server_url))
					throw new FailureException(consumer_id, "check_authentication failed");

				return new ConsumerResponse(consumer_id, query, query["openid.signed"]);
			}

			if (assoc.ExpiresIn <= 0)
			{
				throw new FailureException(consumer_id, String.Format("Association with {0} expired", server_url));
			}

			// Check the signature
			string sig = getRequired("sig");
			string signed = getRequired("signed");
			string[] signed_array = signed.Split(',');

			string v_sig = assoc.SignDict(signed_array, query, "openid.");

			if (v_sig != sig)
				throw new FailureException(consumer_id, "Bad signature");

			return new ConsumerResponse(consumer_id, query, signed);
		}

		private static AssociationRequest CreateAssociationRequest(Uri server_url)
		{
			NameValueCollection args = new NameValueCollection();

			args.Add("openid.mode", "associate");
			args.Add("openid.assoc_type", HMAC_SHA1);

			DiffieHellman dh = null;

			if (server_url.Scheme != Uri.UriSchemeHttps)
			{
				// Initiate Diffie-Hellman Exchange
				dh = CryptUtil.CreateDiffieHellman();

				byte[] dhPublic = dh.CreateKeyExchange();
				string cpub = CryptUtil.UnsignedToBase64(dhPublic);

				args.Add("openid.session_type", DH_SHA1);
				args.Add("openid.dh_consumer_public", cpub);

				DHParameters dhps = dh.ExportParameters(true);

				if (dhps.P != CryptUtil.DEFAULT_MOD || dhps.G != CryptUtil.DEFAULT_GEN)
				{
					args.Add("openid.dh_modulus", CryptUtil.UnsignedToBase64(dhps.P));
					args.Add("openid.dh_gen", CryptUtil.UnsignedToBase64(dhps.G));
				}
			}

			return new AssociationRequest(dh, args);
		}

		private NameValueCollection CreateCheckAuthRequest(NameValueCollection query)
		{
			string signed = query["openid.signed"];

			if (signed == null)
				// #XXX: oidutil.log('No signature present; checkAuth aborted')
				return null;

			// Arguments that are always passed to the server and not
			// included in the signature.
			string[] whitelist = new string[] { "assoc_handle", "sig", "signed", "invalidate_handle" };
			string[] splitted = signed.Split(',');

			string[] signed_array = new string[whitelist.Length + splitted.Length];

			Array.Copy(whitelist, signed_array, 0);
			Array.Copy(splitted, signed_array, splitted.Length);

			NameValueCollection check_args = new NameValueCollection();

			foreach (string key in query.AllKeys)
			{
				if (key.StartsWith("openid.") && Array.IndexOf(signed_array, key.Substring(7)) > -1)
					check_args.Add(key, query[key]);

				check_args["openid.mode"] = "check_authentication";
			}

			return check_args;
		}

		private delegate void DataWriter(string data, bool writeSeparator);

		private string GenToken(ServiceEndpoint endpoint)
		{
			string timestamp = DateTime.UtcNow.ToFileTimeUtc().ToString();

			using (MemoryStream ms = new MemoryStream())
			using (HashAlgorithm sha1 = new HMACSHA1(this.store.AuthKey))
			using (CryptoStream sha1Stream = new CryptoStream(ms, sha1, CryptoStreamMode.Write))
			{
				DataWriter writeData = delegate(string value, bool writeSeparator)
				{
					byte[] buffer = Encoding.ASCII.GetBytes(value);
					sha1Stream.Write(buffer, 0, buffer.Length);

					if (writeSeparator)
						sha1Stream.WriteByte(0);
				};

				writeData(timestamp, true);
				writeData(endpoint.IdentityUrl.AbsoluteUri, true);
				writeData(endpoint.ServerId.AbsoluteUri, true);
				writeData(endpoint.ServerUrl.AbsoluteUri, false);

				sha1Stream.Flush();
				sha1Stream.FlushFinalBlock();

				byte[] hash = sha1.Hash;

				byte[] data = new byte[sha1.HashSize / 8 + ms.Length];
				Buffer.BlockCopy(hash, 0, data, 0, hash.Length);
				Buffer.BlockCopy(ms.ToArray(), 0, data, hash.Length, (int)ms.Length);

				return CryptUtil.ToBase64String(data);
			}
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
				if (getParameter("assoc_type") != HMAC_SHA1)
					// XXX: log this
					return null;

				byte[] secret;

				string session_type = (string)results["session_type"];

				if (session_type == null)
					secret = getDecoded("mac_key");
				else if (session_type == DH_SHA1)
				{
					byte[] dh_server_public = getDecoded("dh_server_public");
					byte[] enc_mac_key = getDecoded("enc_mac_key");
					secret = CryptUtil.SHA1XorSecret(dh, dh_server_public, enc_mac_key);
				}
				else // # XXX: log this
					return null;

				string assocHandle = getParameter("assoc_handle");
				TimeSpan expiresIn = new TimeSpan(0, 0, Convert.ToInt32(getParameter("expires_in")));

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
			string is_valid = (string)response["is_valid"];

			if (is_valid == "true")
			{
				string invalidate_handle = (string)response["invalidate_handle"];
				if (invalidate_handle != null)
					this.store.RemoveAssociation(server_url, invalidate_handle);

				return true;
			}

			// XXX: Log this
			return false;
		}

		private IList SplitToken(string token)
		{
			byte[] tok = Convert.FromBase64String(token);

			if (tok.Length < 20)
				// XXX: log this
				return null;

			byte[] sig = new byte[20];
			Buffer.BlockCopy(tok, 0, sig, 0, 20);

			HMACSHA1 hmac = new HMACSHA1(this.store.AuthKey);
			byte[] newSig = hmac.ComputeHash(tok, 20, tok.Length - 20);

			for (int i = 0; i < sig.Length; i++)
				if (sig[i] != newSig[i])
					return null; // XXX: log this

			List<string> items = new List<string>();

			int prev = 20;
			int idx;

			while ((idx = Array.IndexOf<byte>(tok, 0, prev)) > -1)
			{
				items.Add(Encoding.ASCII.GetString(tok, prev, idx - prev));

				prev = idx + 1;
			}

			if (prev < tok.Length)
				items.Add(Encoding.ASCII.GetString(tok, prev, tok.Length - prev));

			//# Check if timestamp has expired
			DateTime ts = DateTime.FromFileTimeUtc(Convert.ToInt64(items[0]));
			ts += new TimeSpan(0, 0, (int)TOKEN_LIFETIME);

			if (ts < DateTime.UtcNow)
				return null; //    # XXX: log this

			items.RemoveAt(0);

			try
			{
				return items.ConvertAll<Uri>(delegate(string url) { return new Uri(url); });
			}
			catch (UriFormatException)
			{
				return null;
			}
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
