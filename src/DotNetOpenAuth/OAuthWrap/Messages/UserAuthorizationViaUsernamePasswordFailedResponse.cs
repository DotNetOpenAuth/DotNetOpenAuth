//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationViaUsernamePasswordFailedResponse.cs" company="Andrew Arnott">
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
	/// A response from the Authorization Server to the Consumer to indicate that a
	/// request for a delegation code failed, probably due to an invalid
	/// username and password.
	/// </summary>
	internal class UserAuthorizationViaUsernamePasswordFailedResponse : MessageBase, IHttpDirectResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationViaUsernamePasswordFailedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal UserAuthorizationViaUsernamePasswordFailedResponse(UserAuthorizationViaUsernamePasswordRequest request)
			: base(request) {
		}

		#region IHttpDirectResponse Members

		/// <summary>
		/// Gets the HTTP status code that the direct respones should be sent with.
		/// </summary>
		/// <value><see cref="HttpStatusCode.Unauthorized"/></value>
		HttpStatusCode IHttpDirectResponse.HttpStatusCode {
			get { return HttpStatusCode.Unauthorized; }
		}

		#endregion

		/// <summary>
		/// Gets or sets the error reason.
		/// </summary>
		/// <value>
		/// The reason for the failure.  Among other values, it may be <c>null</c>
		/// or invalid_user_credentials.
		/// </value>
		[MessagePart(Protocol.wrap_error_reason, IsRequired = false, AllowEmpty = true)]
		internal string ErrorReason { get; set; }
	}
}
