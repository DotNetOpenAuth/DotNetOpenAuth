//-----------------------------------------------------------------------
// <copyright file="UserNamePasswordVerificationResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using System.Net;

	/// <summary>
	/// A response from the Authorization Server to the client indicating that the user
	/// must visit a URL to complete an additional verification step before proceeding.
	/// </summary>
	internal class UserNamePasswordVerificationResponse : MessageBase, IHttpDirectResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserNamePasswordVerificationResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal UserNamePasswordVerificationResponse(UserNamePasswordRequest request)
			: base(request) {
		}

		#region IHttpDirectResponse Members

		/// <summary>
		/// Gets the HTTP status code that the direct response should be sent with.
		/// </summary>
		/// <value><see cref="HttpStatusCode.BadRequest"/></value>
		HttpStatusCode IHttpDirectResponse.HttpStatusCode {
			get { return HttpStatusCode.BadRequest; }
		}

		/// <summary>
		/// Gets the HTTP headers to add to the response.
		/// </summary>
		WebHeaderCollection IHttpDirectResponse.Headers {
			get { return new WebHeaderCollection(); }
		}

		#endregion

		/// <summary>
		/// Gets or sets the verification URL the user must visit with a browser
		/// to complete some step to defeat automated attacks.
		/// </summary>
		/// <value>The verification URL.</value>
		[MessagePart(Protocol.wrap_verification_url, IsRequired = true, AllowEmpty = false)]
		internal Uri VerificationUrl { get; set; }
	}
}
