namespace Janrain.OpenId.Consumer
{
	using System;
	using System.Net;

	[Serializable]
	public class FetchResponse
	{
		public HttpStatusCode code;
		public Uri finalUri;
		public byte[] data;
		public int length;
		public string charset;

		public FetchResponse(HttpStatusCode code, Uri finalUri, string charset, byte[] data, int length)
		{
			throw new System.NotImplementedException();
		}
	}
}
