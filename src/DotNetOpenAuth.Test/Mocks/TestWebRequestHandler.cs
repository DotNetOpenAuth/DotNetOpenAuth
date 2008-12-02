//-----------------------------------------------------------------------
// <copyright file="TestWebRequestHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.IO;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.ChannelElements;

	internal class TestWebRequestHandler : IDirectSslWebRequestHandler {
		private StringBuilder postEntity;

		/// <summary>
		/// Gets or sets the callback used to provide the mock response for the mock request.
		/// </summary>
		internal Func<HttpWebRequest, DirectWebResponse> Callback { get; set; }

		/// <summary>
		/// Gets the stream that was written out as if on an HTTP request.
		/// </summary>
		internal Stream RequestEntityStream {
			get {
				if (this.postEntity == null) {
					return null;
				}
				return new MemoryStream(Encoding.UTF8.GetBytes(this.postEntity.ToString()));
			}
		}

		/// <summary>
		/// Gets the stream that was written out as if on an HTTP request as an ordinary string.
		/// </summary>
		internal string RequestEntityAsString {
			get {
				return this.postEntity != null ? this.postEntity.ToString() : null;
			}
		}

		#region IWebRequestHandler Members

		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <returns>
		/// The writer the caller should write out the entity data to.
		/// </returns>
		public TextWriter GetRequestStream(HttpWebRequest request) {
			this.postEntity = new StringBuilder();
			return new StringWriter(this.postEntity);
		}

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the
		/// <see cref="HttpWebResponse"/> to a <see cref="Response"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <returns>
		/// An instance of <see cref="Response"/> describing the response.
		/// </returns>
		public DirectWebResponse GetResponse(HttpWebRequest request) {
			if (this.Callback == null) {
				throw new InvalidOperationException("Set the Callback property first.");
			}

			return this.Callback(request);
		}

		#endregion

		#region IDirectSslWebRequestHandler Members

		public TextWriter GetRequestStream(HttpWebRequest request, bool requireSsl) {
			ErrorUtilities.VerifyProtocol(!requireSsl || request.RequestUri.Scheme == Uri.UriSchemeHttps, "disallowed request");
			return this.GetRequestStream(request);
		}

		public DirectWebResponse GetResponse(HttpWebRequest request, bool requireSsl) {
			ErrorUtilities.VerifyProtocol(!requireSsl || request.RequestUri.Scheme == Uri.UriSchemeHttps, "disallowed request");
			var result = this.GetResponse(request);
			ErrorUtilities.VerifyProtocol(!requireSsl || result.FinalUri.Scheme == Uri.UriSchemeHttps, "disallowed request");
			return result;
		}

		#endregion
	}
}
