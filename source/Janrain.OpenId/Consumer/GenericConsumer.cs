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

	public class GenericConsumer
	{
		private static uint TOKEN_LIFETIME = 120;

		private static readonly string DH_SHA1 = "DH-SHA1";
		private static readonly string HMAC_SHA1 = "HMAC-SHA1";

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
			Association assoc = GetAssociation(service_endpoint.ServerUrl);
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
			else if (mode == "error")
			{
				string error = query["openid.error"];
				throw new FailureException(identity_url, error);
			}
			else if (mode == "id_res")
			{
				if (identity_url == null)
					throw new FailureException(identity_url, "No session state found");

				ConsumerResponse response = DoIdRes(query, identity_url, server_id, server_url);
				CheckNonce(response, query["nonce"]);
				return response;
			}
			else
				throw new FailureException(identity_url, "Invalid openid.mode: " + mode);

		}
		private bool CheckAuth(NameValueCollection query, Uri server_url) { throw new NotImplementedException(); }
		private void CheckNonce(ConsumerResponse response, string nonce) { throw new NotImplementedException(); }
		private static object[] CreateAssociationRequest(Uri server_url) { throw new NotImplementedException(); }
		private NameValueCollection CreateCheckAuthRequest(NameValueCollection query) { throw new NotImplementedException(); }
		private ConsumerResponse DoIdRes(NameValueCollection query, Uri consumer_id, Uri server_id, Uri server_url) { throw new NotImplementedException(); }
		private string GenToken(ServiceEndpoint endpoint) { throw new NotImplementedException(); }
		private Association GetAssociation(Uri server_url) { throw new NotImplementedException(); }
		private IDictionary MakeKVPost(NameValueCollection args, Uri server_url) { throw new NotImplementedException(); }
		protected HMACSHA1Association ParseAssociation(IDictionary results, DiffieHellman dh, Uri server_url) { throw new NotImplementedException(); }
		private bool ProcessCheckAuthResponse(IDictionary response, Uri server_url) { throw new NotImplementedException(); }
		private IList SplitToken(string token) { throw new NotImplementedException(); }

	}
}
