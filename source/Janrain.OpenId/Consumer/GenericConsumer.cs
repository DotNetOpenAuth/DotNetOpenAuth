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
		static GenericConsumer()
		{ throw new NotImplementedException(); }
		public GenericConsumer(IAssociationStore store, Fetcher fetcher) { throw new NotImplementedException(); }
		public AuthRequest Begin(ServiceEndpoint service_endpoint) { throw new NotImplementedException(); }
		private bool CheckAuth(NameValueCollection query, Uri server_url) { throw new NotImplementedException(); }
		private void CheckNonce(ConsumerResponse response, string nonce) { throw new NotImplementedException(); }
		public ConsumerResponse Complete(NameValueCollection query, string token) { throw new NotImplementedException(); }
		private static object[] CreateAssociationRequest(Uri server_url) { throw new NotImplementedException(); }
		private NameValueCollection CreateCheckAuthRequest(NameValueCollection query) { throw new NotImplementedException(); }
		private ConsumerResponse DoIdRes(NameValueCollection query, Uri consumer_id, Uri server_id, Uri server_url) { throw new NotImplementedException(); }
		private string GenToken(ServiceEndpoint endpoint) { throw new NotImplementedException(); }
		private Association GetAssociation(Uri server_url) { throw new NotImplementedException(); }
		private IDictionary MakeKVPost(NameValueCollection args, Uri server_url) { throw new NotImplementedException(); }
		protected HMACSHA1Association ParseAssociation(IDictionary results, DiffieHellman dh, Uri server_url) { throw new NotImplementedException(); }
		private bool ProcessCheckAuthResponse(IDictionary response, Uri server_url) { throw new NotImplementedException(); }
		private List<string> SplitToken(string token) { throw new NotImplementedException(); }

	}
}
