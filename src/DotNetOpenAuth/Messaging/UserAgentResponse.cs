//-----------------------------------------------------------------------
// <copyright file="UserAgentResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Net;
	using System.Text;
	using System.Threading;
	using System.Web;

	/// <summary>
	/// A protocol message (request or response) that passes between Consumer and Service Provider
	/// via the user agent using a redirect or form POST submission,
	/// OR a direct message response.
	/// </summary>
	/// <remarks>
	/// <para>An instance of this type describes the HTTP response that must be sent
	/// in response to the current HTTP request.</para>
	/// <para>It is important that this response make up the entire HTTP response.
	/// A hosting ASPX page should not be allowed to render its normal HTML output
	/// after this response is sent.  The normal rendered output of an ASPX page 
	/// can be canceled by calling <see cref="HttpResponse.End"/> after this message
	/// is sent on the response stream.</para>
	/// </remarks>
	public class UserAgentResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentResponse"/> class.
		/// </summary>
		internal UserAgentResponse() {
			this.Status = HttpStatusCode.OK;
			this.Headers = new WebHeaderCollection();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentResponse"/> class
		/// based on the contents of an <see cref="HttpWebResponse"/>.
		/// </summary>
		/// <param name="response">The <see cref="HttpWebResponse"/> to clone.</param>
		/// <param name="maximumBytesToRead">The maximum bytes to read from the response stream.</param>
		protected internal UserAgentResponse(HttpWebResponse response, int maximumBytesToRead) {
			ErrorUtilities.VerifyArgumentNotNull(response, "response");

			this.Status = response.StatusCode;
			this.Headers = response.Headers;
			this.ResponseStream = new MemoryStream(response.ContentLength < 0 ? 4 * 1024 : (int)response.ContentLength);
			using (Stream responseStream = response.GetResponseStream()) {
				// BUGBUG: strictly speaking, is the response were exactly the limit, we'd report it as truncated here.
				this.IsResponseTruncated = responseStream.CopyTo(this.ResponseStream, maximumBytesToRead) == maximumBytesToRead;
				this.ResponseStream.Seek(0, SeekOrigin.Begin);
			}
		}

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
		/// Gets the body of the HTTP response.
		/// </summary>
		public Stream ResponseStream { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether the response stream is incomplete due
		/// to a length limitation imposed by the HttpWebRequest or calling method.
		/// </summary>
		public bool IsResponseTruncated { get; internal set; }

		/// <summary>
		/// Gets or sets the body of the response as a string.
		/// </summary>
		public string Body {
			get { return this.ResponseStream != null ? this.GetResponseReader().ReadToEnd() : null; }
			set { this.SetResponse(value); }
		}

		/// <summary>
		/// Gets the HTTP status code to use in the HTTP response.
		/// </summary>
		public HttpStatusCode Status { get; internal set; }

		/// <summary>
		/// Gets or sets a reference to the actual protocol message that
		/// is being sent via the user agent.
		/// </summary>
		internal IProtocolMessage OriginalMessage { get; set; }

		/// <summary>
		/// Gets the URI that, when requested with an HTTP GET request,
		/// would transmit the message that normally would be transmitted via a user agent redirect.
		/// </summary>
		/// <remarks>
		/// This is useful for desktop applications that will spawn a user agent to transmit the message
		/// rather than cause a redirect.
		/// </remarks>
		internal Uri DirectUriRequest {
			get {
				var message = this.OriginalMessage as IDirectedProtocolMessage;
				if (message == null) {
					throw new InvalidOperationException(); // this only makes sense for directed messages (indirect responses)
				}

				var serializer = MessageSerializer.Get(message.GetType());
				var fields = serializer.Serialize(message);
				UriBuilder builder = new UriBuilder(message.Recipient);
				MessagingUtilities.AppendQueryArgs(builder, fields);
				return builder.Uri;
			}
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
		/// Automatically sends the appropriate response to the user agent
		/// and ends execution on the current page or handler.
		/// </summary>
		/// <exception cref="ThreadAbortException">Thrown by ASP.NET in order to prevent additional data from the page being sent to the client and corrupting the response.</exception>
		/// <remarks>
		/// Requires a current HttpContext.
		/// </remarks>
		public virtual void Send() {
			ErrorUtilities.VerifyOperation(HttpContext.Current != null, MessagingStrings.CurrentHttpContextRequired);

			HttpContext.Current.Response.Clear();
			HttpContext.Current.Response.StatusCode = (int)this.Status;
			MessagingUtilities.ApplyHeadersToResponse(this.Headers, HttpContext.Current.Response);
			if (this.ResponseStream != null) {
				try {
					this.ResponseStream.CopyTo(HttpContext.Current.Response.OutputStream);
				} catch (HttpException ex) {
					if (ex.ErrorCode == -2147467259 && HttpContext.Current.Response.Output != null) {
						// Test scenarios can generate this, since the stream is being spoofed:
						// System.Web.HttpException: OutputStream is not available when a custom TextWriter is used.
						HttpContext.Current.Response.Output.Write(this.Body);
					} else {
						throw;
					}
				}
			}
			HttpContext.Current.Response.End();
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
	}
}
