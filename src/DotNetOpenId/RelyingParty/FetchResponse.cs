namespace DotNetOpenId.RelyingParty {
	using System;
	using System.Net;
	using System.Collections.Specialized;
	using System.IO;
	using System.Text;
	using System.Net.Mime;
	using System.Diagnostics;

	[Serializable]
	[DebuggerDisplay("{StatusCode} {ContentType.MediaType}: {ReadResponseString().Substring(4,50)}")]
	internal class FetchResponse {
		const string DefaultContentEncoding = "ISO-8859-1";
		
		public Stream ResponseStream { get; private set; }
		public HttpStatusCode StatusCode { get; private set; }
		public ContentType ContentType { get; private set; }
		public string ContentEncoding { get; private set; }
		public WebHeaderCollection Headers { get; private set; }
		public Uri RequestUri { get; private set; }
		public Uri FinalUri { get; private set; }

		public FetchResponse(Uri requestUri, HttpWebResponse response, Stream responseStream) {
			if (requestUri == null) throw new ArgumentNullException("requestUri");
			if (response == null) throw new ArgumentNullException("response");
			if (responseStream == null) throw new ArgumentNullException("responseStream");
			this.RequestUri = requestUri;
			this.ResponseStream = responseStream;
			StatusCode = response.StatusCode;
			ContentType = new ContentType(response.ContentType);
			ContentEncoding = string.IsNullOrEmpty(response.ContentEncoding) ? DefaultContentEncoding : response.ContentEncoding;
			Headers = response.Headers;
			FinalUri = response.ResponseUri;
		}

		public string ReadResponseString() {
			// We do NOT put a using clause around this or dispose of the StreamReader
			// because that would dispose of the underlying stream, preventing this
			// method from being called again.
			StreamReader sr = new StreamReader(ResponseStream, Encoding.GetEncoding(ContentEncoding));
			long oldPosition = ResponseStream.Position;
			string result = sr.ReadToEnd();
			ResponseStream.Seek(oldPosition, SeekOrigin.Begin);
			return result;
		}
	}
}
