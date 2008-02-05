namespace DotNetOpenId.Consumer
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
			this.code = code;
			this.finalUri = finalUri;
			this.data = data;
			this.length = length;
			this.charset = charset;
		}
	}
}
