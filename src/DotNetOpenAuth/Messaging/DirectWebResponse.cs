//-----------------------------------------------------------------------
// <copyright file="DirectWebResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Net.Mime;
	using System.Text;

	/// <summary>
	/// Details on the response from a direct web request to a remote party.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("{Status} {ContentType.MediaType}: {Body.Substring(4,50)}")]
	public class DirectWebResponse : IDisposable {
		/// <summary>
		/// The encoding to use in reading a response that does not declare its own content encoding.
		/// </summary>
		private const string DefaultContentEncoding = "ISO-8859-1";

		/// <summary>
		/// The network response object, used to initialize this instance, that still needs 
		/// to be closed if applicable.
		/// </summary>
		private HttpWebResponse httpWebResponse;

		/// <summary>
		/// An object to be locked whenever the <see cref="httpWebResponse"/> or the
		/// <see cref="ResponseStream"/> members are being accessed.
		/// </summary>
		private object responseLock = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectWebResponse"/> class.
		/// </summary>
		internal DirectWebResponse() {
			this.Status = HttpStatusCode.OK;
			this.Headers = new WebHeaderCollection();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectWebResponse"/> class.
		/// </summary>
		/// <param name="requestUri">The original request URI.</param>
		/// <param name="response">The response to initialize from.  The network stream is used by this class directly.</param>
		internal DirectWebResponse(Uri requestUri, HttpWebResponse response) {
			ErrorUtilities.VerifyArgumentNotNull(requestUri, "requestUri");
			ErrorUtilities.VerifyArgumentNotNull(response, "response");

			this.RequestUri = requestUri;
			if (!string.IsNullOrEmpty(response.ContentType)) {
				this.ContentType = new ContentType(response.ContentType);
			}
			this.ContentEncoding = string.IsNullOrEmpty(response.ContentEncoding) ? DefaultContentEncoding : response.ContentEncoding;
			this.FinalUri = response.ResponseUri;
			this.Status = response.StatusCode;
			this.Headers = response.Headers;
			this.httpWebResponse = response;
			this.ResponseStream = response.GetResponseStream();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectWebResponse"/> class.
		/// </summary>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="responseUri">The final URI to respond to the request.</param>
		/// <param name="headers">The headers.</param>
		/// <param name="statusCode">The status code.</param>
		/// <param name="contentType">Type of the content.</param>
		/// <param name="contentEncoding">The content encoding.</param>
		/// <param name="responseStream">The response stream.</param>
		internal DirectWebResponse(Uri requestUri, Uri responseUri, WebHeaderCollection headers, HttpStatusCode statusCode, string contentType, string contentEncoding, Stream responseStream) {
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

		/// <summary>
		/// Gets the type of the content.
		/// </summary>
		public ContentType ContentType { get; private set; }

		/// <summary>
		/// Gets the content encoding.
		/// </summary>
		public string ContentEncoding { get; private set; }

		/// <summary>
		/// Gets the URI of the initial request.
		/// </summary>
		public Uri RequestUri { get; private set; }

		/// <summary>
		/// Gets the URI that finally responded to the request.
		/// </summary>
		/// <remarks>
		/// This can be different from the <see cref="RequestUri"/> in cases of 
		/// redirection during the request.
		/// </remarks>
		public Uri FinalUri { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the response stream is incomplete due
		/// to a length limitation imposed by the HttpWebRequest or calling method.
		/// </summary>
		public bool IsResponseTruncated { get; internal set; }

		/// <summary>
		/// Gets the headers that must be included in the response to the user agent.
		/// </summary>
		/// <remarks>
		/// The headers in this collection are not meant to be a comprehensive list
		/// of exactly what should be sent, but are meant to augment whatever headers
		/// are generally included in a typical response.
		/// </remarks>
		public WebHeaderCollection Headers { get; internal set; }

		/// <summary>
		/// Gets the HTTP status code to use in the HTTP response.
		/// </summary>
		public HttpStatusCode Status { get; internal set; }

		/// <summary>
		/// Gets the body of the HTTP response.
		/// </summary>
		public Stream ResponseStream { get; internal set; }

		/// <summary>
		/// Gets or sets the body of the response as a string.
		/// </summary>
		public string Body {
			get { return this.ResponseStream != null ? this.GetResponseReader().ReadToEnd() : null; }
			set { this.SetResponse(value); }
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
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

		/// <summary>
		/// Creates a text reader for the response stream.
		/// </summary>
		/// <returns>The text reader, initialized for the proper encoding.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Costly operation")]
		public StreamReader GetResponseReader() {
			this.ResponseStream.Seek(0, SeekOrigin.Begin);
			string contentEncoding = this.Headers[HttpResponseHeader.ContentEncoding];
			if (string.IsNullOrEmpty(contentEncoding)) {
				return new StreamReader(this.ResponseStream);
			} else {
				return new StreamReader(this.ResponseStream, Encoding.GetEncoding(contentEncoding));
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(true);
		}

		/// <summary>
		/// Caches the network stream and closes it if it is open.
		/// </summary>
		internal void CacheNetworkStreamAndClose() {
			this.CacheNetworkStreamAndClose(int.MaxValue);
		}

		/// <summary>
		/// Caches the network stream and closes it if it is open.
		/// </summary>
		/// <param name="maximumBytesToRead">The maximum bytes to cache.</param>
		internal void CacheNetworkStreamAndClose(int maximumBytesToRead) {
			lock (this.responseLock) {
				if (this.httpWebResponse != null) {
					// Now read and cache the network stream
					Stream networkStream = this.ResponseStream;
					this.ResponseStream = new MemoryStream(this.httpWebResponse.ContentLength < 0 ? 4 * 1024 : Math.Min((int)this.httpWebResponse.ContentLength, maximumBytesToRead));
					//// BUGBUG: strictly speaking, is the response were exactly the limit, we'd report it as truncated here.
					this.IsResponseTruncated = networkStream.CopyTo(this.ResponseStream, maximumBytesToRead) == maximumBytesToRead;
					this.ResponseStream.Seek(0, SeekOrigin.Begin);

					networkStream.Dispose();
					this.httpWebResponse.Close();
					this.httpWebResponse = null;
				}
			}
		}

		/// <summary>
		/// Sets the response to some string, encoded as UTF-8.
		/// </summary>
		/// <param name="body">The string to set the response to.</param>
		internal void SetResponse(string body) {
			if (body == null) {
				this.ResponseStream = null;
				return;
			}

			Encoding encoding = Encoding.UTF8;
			this.Headers[HttpResponseHeader.ContentEncoding] = encoding.HeaderName;
			this.ResponseStream = new MemoryStream();
			StreamWriter writer = new StreamWriter(this.ResponseStream, encoding);
			writer.Write(body);
			writer.Flush();
			this.ResponseStream.Seek(0, SeekOrigin.Begin);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected void Dispose(bool disposing) {
			if (disposing) {
				lock (this.responseLock) {
					if (this.ResponseStream != null) {
						this.ResponseStream.Dispose();
						this.ResponseStream = null;
					}
					if (this.httpWebResponse != null) {
						this.httpWebResponse.Close();
						this.httpWebResponse = null;
					}
				}
			}
		}
	}
}
