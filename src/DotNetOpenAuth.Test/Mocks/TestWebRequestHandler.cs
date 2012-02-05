//-----------------------------------------------------------------------
// <copyright file="TestWebRequestHandler.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.IO;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.ChannelElements;

	internal class TestWebRequestHandler : IDirectWebRequestHandler {
		private Stream postEntity;

		/// <summary>
		/// Gets or sets the callback used to provide the mock response for the mock request.
		/// </summary>
		internal Func<HttpWebRequest, IncomingWebResponse> Callback { get; set; }

		/// <summary>
		/// Gets the stream that was written out as if on an HTTP request.
		/// </summary>
		internal Stream RequestEntityStream {
			get {
				if (this.postEntity == null) {
					return null;
				}

				Stream result = new MemoryStream();
				long originalPosition = this.postEntity.Position;
				this.postEntity.Position = 0;
				this.postEntity.CopyTo(result);
				this.postEntity.Position = originalPosition;
				result.Position = 0;
				return result;
			}
		}

		/// <summary>
		/// Gets the stream that was written out as if on an HTTP request as an ordinary string.
		/// </summary>
		internal string RequestEntityAsString {
			get {
				if (this.postEntity == null) {
					return null;
				}

				StreamReader reader = new StreamReader(this.RequestEntityStream);
				return reader.ReadToEnd();
			}
		}

		#region IWebRequestHandler Members

		public bool CanSupport(DirectWebRequestOptions options) {
			return true;
		}

		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <returns>
		/// The writer the caller should write out the entity data to.
		/// </returns>
		public Stream GetRequestStream(HttpWebRequest request) {
			return this.GetRequestStream(request, DirectWebRequestOptions.None);
		}

		public Stream GetRequestStream(HttpWebRequest request, DirectWebRequestOptions options) {
			this.postEntity = new MemoryStream();
			return this.postEntity;
		}

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the
		/// <see cref="HttpWebResponse"/> to a <see cref="Response"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <returns>
		/// An instance of <see cref="Response"/> describing the response.
		/// </returns>
		public IncomingWebResponse GetResponse(HttpWebRequest request) {
			return this.GetResponse(request, DirectWebRequestOptions.None);
		}

		public IncomingWebResponse GetResponse(HttpWebRequest request, DirectWebRequestOptions options) {
			if (this.Callback == null) {
				throw new InvalidOperationException("Set the Callback property first.");
			}

			return this.Callback(request);
		}

		#endregion

		#region IDirectSslWebRequestHandler Members

		public Stream GetRequestStream(HttpWebRequest request, bool requireSsl) {
			ErrorUtilities.VerifyProtocol(!requireSsl || request.RequestUri.Scheme == Uri.UriSchemeHttps, "disallowed request");
			return this.GetRequestStream(request);
		}

		public IncomingWebResponse GetResponse(HttpWebRequest request, bool requireSsl) {
			ErrorUtilities.VerifyProtocol(!requireSsl || request.RequestUri.Scheme == Uri.UriSchemeHttps, "disallowed request");
			var result = this.GetResponse(request);
			ErrorUtilities.VerifyProtocol(!requireSsl || result.FinalUri.Scheme == Uri.UriSchemeHttps, "disallowed request");
			return result;
		}

		#endregion
	}
}
