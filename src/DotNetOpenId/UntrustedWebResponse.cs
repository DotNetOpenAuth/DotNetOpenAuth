namespace DotNetOpenId {
	using System;
	using System.Net;
	using System.Collections.Specialized;
	using System.IO;
	using System.Text;
	using System.Net.Mime;
	using System.Diagnostics;
	using System.Globalization;

	[Serializable]
	[DebuggerDisplay("{StatusCode} {ContentType.MediaType}: {ReadResponseString().Substring(4,50)}")]
	internal class UntrustedWebResponse {
		const string DefaultContentEncoding = "ISO-8859-1";
		
		public Stream ResponseStream { get; private set; }
		public HttpStatusCode StatusCode { get; private set; }
		public ContentType ContentType { get; private set; }
		public string ContentEncoding { get; private set; }
		public WebHeaderCollection Headers { get; private set; }
		public Uri RequestUri { get; private set; }
		public Uri FinalUri { get; private set; }

		public UntrustedWebResponse(Uri requestUri, HttpWebResponse response, Stream responseStream) {
			if (requestUri == null) throw new ArgumentNullException("requestUri");
			if (response == null) throw new ArgumentNullException("response");
			if (responseStream == null) throw new ArgumentNullException("responseStream");
			this.RequestUri = requestUri;
			this.ResponseStream = responseStream;
			StatusCode = response.StatusCode;
			if (!string.IsNullOrEmpty(response.ContentType)) {
				try {
					ContentType = new ContentType(response.ContentType);
				} catch (FormatException) {
					Logger.ErrorFormat("HTTP response to {0} included an invalid Content-Type header value: {1}", response.ResponseUri.AbsoluteUri, response.ContentType);
				}
			}
			ContentEncoding = string.IsNullOrEmpty(response.ContentEncoding) ? DefaultContentEncoding : response.ContentEncoding;
			Headers = response.Headers;
			FinalUri = response.ResponseUri;
		}

		/// <summary>
		/// Constructs a mock web response.
		/// </summary>
		internal UntrustedWebResponse(Uri requestUri, Uri responseUri, WebHeaderCollection headers,
			HttpStatusCode statusCode, string contentType, string contentEncoding, Stream responseStream) {
			if (requestUri == null) throw new ArgumentNullException("requestUri");
			if (responseStream == null) throw new ArgumentNullException("responseStream");
			RequestUri = requestUri;
			ResponseStream = responseStream;
			StatusCode = statusCode;
			if (!string.IsNullOrEmpty(contentType)) {
				try {
					ContentType = new ContentType(contentType);
				} catch (FormatException) {
					Logger.ErrorFormat("HTTP response to {0} included an invalid Content-Type header value: {1}", responseUri.AbsoluteUri, contentType);
				}
			}
			ContentEncoding = string.IsNullOrEmpty(contentEncoding) ? DefaultContentEncoding : contentEncoding;
			Headers = headers;
			FinalUri = responseUri;
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

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "RequestUri = {0}", RequestUri));
			sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "ResponseUri = {0}", FinalUri));
			sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "StatusCode = {0}", StatusCode));
			sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "ContentType = {0}", ContentType));
			sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "ContentEncoding = {0}", ContentEncoding));
			sb.AppendLine("Headers:");
			foreach (string header in Headers) {
				sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "\t{0}: {1}", header, Headers[header]));
			}
			sb.AppendLine("Response:");
			sb.AppendLine(ReadResponseString());
			return sb.ToString();
		}
	}
}
