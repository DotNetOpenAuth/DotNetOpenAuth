//-----------------------------------------------------------------------
// <copyright file="OAuth1PlainTextMessageHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A delegating HTTP handler that signs outgoing HTTP requests
	/// with the PLAINTEXT signature.
	/// </summary>
	public class OAuth1PlainTextMessageHandler : OAuth1HttpMessageHandlerBase {
		/// <summary>
		/// Gets the signature method to include in the oauth_signature_method parameter.
		/// </summary>
		/// <value>
		/// The signature method.
		/// </value>
		protected override string SignatureMethod {
			get { return "PLAINTEXT"; }
		}

		/// <summary>
		/// Calculates the signature for the specified buffer.
		/// </summary>
		/// <param name="signedPayload">The payload to calculate the signature for.</param>
		/// <returns>
		/// The signature.
		/// </returns>
		/// <exception cref="System.NotImplementedException">Always thrown.</exception>
		protected override byte[] Sign(byte[] signedPayload) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the OAuth 1.0 signature to apply to the specified request.
		/// </summary>
		/// <param name="request">The outbound HTTP request.</param>
		/// <param name="oauthParameters">The oauth parameters.</param>
		/// <returns>
		/// The value for the "oauth_signature" parameter.
		/// </returns>
		protected override string GetSignature(System.Net.Http.HttpRequestMessage request, NameValueCollection oauthParameters) {
			var builder = new StringBuilder();
			builder.Append(MessagingUtilities.EscapeUriDataStringRfc3986(this.ConsumerSecret));
			builder.Append("&");
			builder.Append(MessagingUtilities.EscapeUriDataStringRfc3986(this.AccessTokenSecret));
			return builder.ToString();
		}
	}
}
