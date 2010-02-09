//-----------------------------------------------------------------------
// <copyright file="WebAppAccessTokenFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A response from the Authorization Server to the Client in the event
	/// that the <see cref="WebAppAccessTokenRequest"/> message had an
	/// invalid calback URL or verification code.
	/// </summary>
	internal class WebAppAccessTokenFailedResponse : MessageBase, IHttpDirectResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppAccessTokenFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal WebAppAccessTokenFailedResponse(WebAppAccessTokenRequest request)
			: base(request) {
		}

		#region IHttpDirectResponse Members

		/// <summary>
		/// Gets the HTTP status code that the direct response should be sent with.
		/// </summary>
		/// <value></value>
		HttpStatusCode IHttpDirectResponse.HttpStatusCode {
			get { return System.Net.HttpStatusCode.BadRequest; }
		}

		/// <summary>
		/// Gets the HTTP headers to add to the response.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		WebHeaderCollection IHttpDirectResponse.Headers {
			get { return new WebHeaderCollection(); }
		}

		#endregion

		/// <summary>
		/// Gets or sets the error reason.
		/// </summary>
		/// <value>"expired_verification_code" or "invalid_callback".</value>
		[MessagePart(Protocol.wrap_error_reason, IsRequired = false, AllowEmpty = true)]
		internal string ErrorReason { get; set; }
	}
}
