//-----------------------------------------------------------------------
// <copyright file="OAuth1HmacSha1HttpMessageHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// A delegating HTTP handler that signs outgoing HTTP requests 
	/// with an HMAC-SHA1 signature.
	/// </summary>
	public class OAuth1HmacSha1HttpMessageHandler : OAuth1HttpMessageHandlerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth1HmacSha1HttpMessageHandler"/> class.
		/// </summary>
		public OAuth1HmacSha1HttpMessageHandler() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth1HmacSha1HttpMessageHandler"/> class.
		/// </summary>
		/// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
		public OAuth1HmacSha1HttpMessageHandler(HttpMessageHandler innerHandler)
			: base(innerHandler) {
		}

		/// <summary>
		/// Gets the signature method to include in the oauth_signature_method parameter.
		/// </summary>
		/// <value>
		/// The signature method.
		/// </value>
		protected override string SignatureMethod {
			get { return "HMAC-SHA1"; }
		}

		/// <summary>
		/// Calculates the signature for the specified buffer.
		/// </summary>
		/// <param name="signedPayload">The payload to calculate the signature for.</param>
		/// <returns>
		/// The signature.
		/// </returns>
		protected override byte[] Sign(byte[] signedPayload) {
			using (var algorithm = HMACSHA1.Create()) {
				algorithm.Key = Encoding.ASCII.GetBytes(this.GetConsumerAndTokenSecretString());
				return algorithm.ComputeHash(signedPayload);
			}
		}
	}
}
