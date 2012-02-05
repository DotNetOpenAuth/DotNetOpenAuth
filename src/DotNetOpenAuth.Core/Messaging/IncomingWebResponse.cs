//-----------------------------------------------------------------------
// <copyright file="IncomingWebResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Net.Mime;
	using System.Text;

	/// <summary>
	/// Details on the incoming response from a direct web request to a remote party.
	/// </summary>
	[ContractVerification(true)]
	[ContractClass(typeof(IncomingWebResponseContract))]
	public abstract class IncomingWebResponse : IDisposable {
		/// <summary>
		/// The encoding to use in reading a response that does not declare its own content encoding.
		/// </summary>
		private const string DefaultContentEncoding = "ISO-8859-1";

		/// <summary>
		/// Initializes a new instance of the <see cref="IncomingWebResponse"/> class.
		/// </summary>
		protected internal IncomingWebResponse() {
			this.Status = HttpStatusCode.OK;
			this.Headers = new WebHeaderCollection();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IncomingWebResponse"/> class.
		/// </summary>
		/// <param name="requestUri">The original request URI.</param>
		/// <param name="response">The response to initialize from.  The network stream is used by this class directly.</param>
		protected IncomingWebResponse(Uri requestUri, HttpWebResponse response) {
			Requires.NotNull(requestUri, "requestUri");
			Requires.NotNull(response, "response");

			this.RequestUri = requestUri;
			if (!string.IsNullOrEmpty(response.ContentType)) {
				try {
					this.ContentType = new ContentType(response.ContentType);
				} catch (FormatException) {
					Logger.Messaging.ErrorFormat("HTTP response to {0} included an invalid Content-Type header value: {1}", response.ResponseUri.AbsoluteUri, response.ContentType);
				}
			}
			this.ContentEncoding = string.IsNullOrEmpty(response.ContentEncoding) ? DefaultContentEncoding : response.ContentEncoding;
			this.FinalUri = response.ResponseUri;
			this.Status = response.StatusCode;
			this.Headers = response.Headers;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IncomingWebResponse"/> class.
		/// </summary>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="responseUri">The final URI to respond to the request.</param>
		/// <param name="headers">The headers.</param>
		/// <param name="statusCode">The status code.</param>
		/// <param name="contentType">Type of the content.</param>
		/// <param name="contentEncoding">The content encoding.</param>
		protected IncomingWebResponse(Uri requestUri, Uri responseUri, WebHeaderCollection headers, HttpStatusCode statusCode, string contentType, string contentEncoding) {
			Requires.NotNull(requestUri, "requestUri");

			this.RequestUri = requestUri;
			this.Status = statusCode;
			if (!string.IsNullOrEmpty(contentType)) {
				try {
					this.ContentType = new ContentType(contentType);
				} catch (FormatException) {
					Logger.Messaging.ErrorFormat("HTTP response to {0} included an invalid Content-Type header value: {1}", responseUri.AbsoluteUri, contentType);
				}
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
		public Uri FinalUri { get; internal set; }

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
		public abstract Stream ResponseStream { get; }

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

			return sb.ToString();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Creates a text reader for the response stream.
		/// </summary>
		/// <returns>The text reader, initialized for the proper encoding.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Costly operation")]
		public abstract StreamReader GetResponseReader();

		/// <summary>
		/// Gets an offline snapshot version of this instance.
		/// </summary>
		/// <param name="maximumBytesToCache">The maximum bytes from the response stream to cache.</param>
		/// <returns>A snapshot version of this instance.</returns>
		/// <remarks>
		/// If this instance is a <see cref="NetworkDirectWebResponse"/> creating a snapshot
		/// will automatically close and dispose of the underlying response stream.
		/// If this instance is a <see cref="CachedDirectWebResponse"/>, the result will
		/// be the self same instance.
		/// </remarks>
		internal abstract CachedDirectWebResponse GetSnapshot(int maximumBytesToCache);

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				Stream responseStream = this.ResponseStream;
				if (responseStream != null) {
					responseStream.Dispose();
				}
			}
		}
	}
}
