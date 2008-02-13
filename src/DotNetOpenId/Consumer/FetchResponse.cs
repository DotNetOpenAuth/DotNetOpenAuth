namespace DotNetOpenId.Consumer
{
	using System;
	using System.Net;

	[Serializable]
	public class FetchResponse
	{
		public HttpStatusCode Code { get; private set; }
		public Uri FinalUri { get; private set; }
		public byte[] Data { get; private set; }
		public int Length { get; private set; }
		public string Charset { get; private set; }

		public FetchResponse(HttpStatusCode code, Uri finalUri, string charset, byte[] data, int length)
		{
			Code = code;
			FinalUri = finalUri;
			Data = data;
			Length = length;
			Charset = charset;
		}
	}
}
