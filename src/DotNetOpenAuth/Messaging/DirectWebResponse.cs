namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Diagnostics;
	using System.IO;
	using System.Globalization;
	using System.Net.Mime;
	using System.Net;

	[Serializable]
	[DebuggerDisplay("{StatusCode} {ContentType.MediaType}: {ReadResponseString().Substring(4,50)}")]
	public class DirectWebResponse : Response {
		private const string DefaultContentEncoding = "ISO-8859-1";

		internal DirectWebResponse() {
		}

		internal DirectWebResponse(Uri requestUri, HttpWebResponse response)
			: this(requestUri, response, int.MaxValue) {
		}

		internal DirectWebResponse(Uri requestUri, HttpWebResponse response, int maximumBytesToRead) : base(response, maximumBytesToRead) {
			ErrorUtilities.VerifyArgumentNotNull(requestUri, "requestUri");
			ErrorUtilities.VerifyArgumentNotNull(response, "response");
			this.RequestUri = requestUri;
			if (!string.IsNullOrEmpty(response.ContentType))
				ContentType = new ContentType(response.ContentType);
			ContentEncoding = string.IsNullOrEmpty(response.ContentEncoding) ? DefaultContentEncoding : response.ContentEncoding;
			FinalUri = response.ResponseUri;
		}

		/// <summary>
		/// Constructs a mock web response.
		/// </summary>
		internal DirectWebResponse(Uri requestUri, Uri responseUri, WebHeaderCollection headers,
			HttpStatusCode statusCode, string contentType, string contentEncoding, Stream responseStream) {
			ErrorUtilities.VerifyArgumentNotNull(requestUri, "requestUri");
			ErrorUtilities.VerifyArgumentNotNull(responseStream, "responseStream");
			this.RequestUri = requestUri;
			this.ResponseStream = responseStream;
			this.Status = statusCode;
			if (!string.IsNullOrEmpty(contentType)) {
				this.ContentType = new ContentType(contentType);
			}
			this.ContentEncoding = string.IsNullOrEmpty(contentEncoding) ? DefaultContentEncoding : contentEncoding;
			this.Headers = headers;
			this.FinalUri = responseUri;
		}

		public ContentType ContentType { get; private set; }
		public string ContentEncoding { get; private set; }
		public Uri RequestUri { get; private set; }
		public Uri FinalUri { get; private set; }

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "RequestUri = {0}", this.RequestUri));
			sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "ResponseUri = {0}", this.FinalUri));
			sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "StatusCode = {0}", this.Status));
			sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "ContentType = {0}", this.ContentType));
			sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "ContentEncoding = {0}", this.ContentEncoding));
			sb.AppendLine("Headers:");
			foreach (string header in this.Headers) {
				sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "\t{0}: {1}", header, this.Headers[header]));
			}
			sb.AppendLine("Response:");
			sb.AppendLine(this.Body);
			return sb.ToString();
		}
	}
}
