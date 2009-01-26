//-----------------------------------------------------------------------
// <copyright file="CachedDirectWebResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Net;
	using System.Text;

	/// <summary>
	/// Cached details on the response from a direct web request to a remote party.
	/// </summary>
	[DebuggerDisplay("{Status} {ContentType.MediaType}: {Body.Substring(4,50)}")]
	internal class CachedDirectWebResponse : DirectWebResponse {
		/// <summary>
		/// A seekable, repeatable response stream.
		/// </summary>
		private MemoryStream responseStream;

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedDirectWebResponse"/> class.
		/// </summary>
		internal CachedDirectWebResponse() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedDirectWebResponse"/> class.
		/// </summary>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="response">The response.</param>
		/// <param name="maximumBytesToRead">The maximum bytes to read.</param>
		internal CachedDirectWebResponse(Uri requestUri, HttpWebResponse response, int maximumBytesToRead)
			: base(requestUri, response) {
			this.responseStream = CacheNetworkStreamAndClose(response, maximumBytesToRead);

			// BUGBUG: if the response was exactly maximumBytesToRead, we'll incorrectly believe it was truncated.
			this.ResponseTruncated = this.responseStream.Length == maximumBytesToRead;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedDirectWebResponse"/> class.
		/// </summary>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="responseUri">The final URI to respond to the request.</param>
		/// <param name="headers">The headers.</param>
		/// <param name="statusCode">The status code.</param>
		/// <param name="contentType">Type of the content.</param>
		/// <param name="contentEncoding">The content encoding.</param>
		/// <param name="responseStream">The response stream.</param>
		internal CachedDirectWebResponse(Uri requestUri, Uri responseUri, WebHeaderCollection headers, HttpStatusCode statusCode, string contentType, string contentEncoding, MemoryStream responseStream)
			: base(requestUri, responseUri, headers, statusCode, contentType, contentEncoding) {
			ErrorUtilities.VerifyArgumentNotNull(responseStream, "responseStream");

			this.responseStream = responseStream;
		}

		/// <summary>
		/// Gets or sets the body of the response as a string.
		/// </summary>
		public string Body {
			get { return this.ResponseStream != null ? this.GetResponseReader().ReadToEnd() : null; }
			set { this.SetResponse(value); }
		}

		/// <summary>
		/// Gets a value indicating whether the cached response stream was
		/// truncated to a maximum allowable length.
		/// </summary>
		public bool ResponseTruncated { get; private set; }

		/// <summary>
		/// Gets the body of the HTTP response.
		/// </summary>
		public override Stream ResponseStream {
			get { return this.responseStream; }
		}

		/// <summary>
		/// Gets or sets the cached response stream.
		/// </summary>
		internal MemoryStream CachedResponseStream {
			get { return this.responseStream; }
			set { this.responseStream = value; }
		}

		/// <summary>
		/// Creates a text reader for the response stream.
		/// </summary>
		/// <returns>The text reader, initialized for the proper encoding.</returns>
		public override StreamReader GetResponseReader() {
			this.ResponseStream.Seek(0, SeekOrigin.Begin);
			string contentEncoding = this.Headers[HttpResponseHeader.ContentEncoding];
			if (string.IsNullOrEmpty(contentEncoding)) {
				return new StreamReader(this.ResponseStream);
			} else {
				return new StreamReader(this.ResponseStream, Encoding.GetEncoding(contentEncoding));
			}
		}

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
		internal override CachedDirectWebResponse GetSnapshot(int maximumBytesToCache) {
			return this;
		}

		/// <summary>
		/// Sets the response to some string, encoded as UTF-8.
		/// </summary>
		/// <param name="body">The string to set the response to.</param>
		internal void SetResponse(string body) {
			if (body == null) {
				this.responseStream = null;
				return;
			}

			Encoding encoding = Encoding.UTF8;
			this.Headers[HttpResponseHeader.ContentEncoding] = encoding.HeaderName;
			this.responseStream = new MemoryStream();
			StreamWriter writer = new StreamWriter(this.ResponseStream, encoding);
			writer.Write(body);
			writer.Flush();
			this.ResponseStream.Seek(0, SeekOrigin.Begin);
		}

		/// <summary>
		/// Caches the network stream and closes it if it is open.
		/// </summary>
		/// <param name="response">The response whose stream is to be cloned.</param>
		/// <param name="maximumBytesToRead">The maximum bytes to cache.</param>
		/// <returns>The seekable Stream instance that contains a copy of what was returned in the HTTP response.</returns>
		private static MemoryStream CacheNetworkStreamAndClose(HttpWebResponse response, int maximumBytesToRead) {
			ErrorUtilities.VerifyArgumentNotNull(response, "response");

			// Now read and cache the network stream
			Stream networkStream = response.GetResponseStream();
			MemoryStream cachedStream = new MemoryStream(response.ContentLength < 0 ? 4 * 1024 : Math.Min((int)response.ContentLength, maximumBytesToRead));
			networkStream.CopyTo(cachedStream);
			cachedStream.Seek(0, SeekOrigin.Begin);

			networkStream.Dispose();
			response.Close();

			return cachedStream;
		}
	}
}
