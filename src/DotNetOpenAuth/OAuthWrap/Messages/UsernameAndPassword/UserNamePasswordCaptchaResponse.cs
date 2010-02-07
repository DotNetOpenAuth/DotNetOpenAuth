//-----------------------------------------------------------------------
// <copyright file="UsernamePasswordCaptchaResponse.cs" company="Andrew Arnott">
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
	/// A response from the Authorization Server indicating the Client must have the user
	/// complete a CAPTCHA puzzle prior to authorization.
	/// </summary>
	internal class UsernamePasswordCaptchaResponse : MessageBase, IHttpDirectResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="UsernamePasswordCaptchaResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal UsernamePasswordCaptchaResponse(UserNamePasswordRequest request)
			: base(request) {
		}

		#region IHttpDirectResponse Members

		HttpStatusCode IHttpDirectResponse.HttpStatusCode {
			get { return HttpStatusCode.BadRequest; }
		}

		WebHeaderCollection IHttpDirectResponse.Headers {
			get { return new WebHeaderCollection(); }
		}

		#endregion

		/// <summary>
		/// Gets or sets the URL to the CAPTCHA puzzle.
		/// </summary>
		/// <value>The captcha URL.</value>
		[MessagePart(Protocol.wrap_captcha_url, IsRequired = true, AllowEmpty = false)]
		internal Uri CaptchaPuzzle { get; set; }
	}
}
