namespace Janrain.OpenId.Consumer
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;

	public class ConsumerResponse
	{
		Uri identity_url;
		public Uri IdentityUrl { get { throw new NotImplementedException(); } }

		IDictionary signed_args;

		public Uri ReturnTo { get { throw new NotImplementedException(); } }

		static ConsumerResponse() { throw new NotImplementedException(); }
		public ConsumerResponse(Uri identity_url, NameValueCollection query, string signed) { throw new NotImplementedException(); }
		public IDictionary ExtensionResponse(string prefix) { throw new NotImplementedException(); }

	}
}
