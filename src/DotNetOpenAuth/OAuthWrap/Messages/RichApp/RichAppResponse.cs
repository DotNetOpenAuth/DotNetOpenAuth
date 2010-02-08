//-----------------------------------------------------------------------
// <copyright file="RichAppResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages.RichApp {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An indirect response from the Authorization Server to the rich app Client
	/// with the verification code.
	/// </summary>
	internal class RichAppResponse : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="RichAppResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The client callback.</param>
		/// <param name="version">The version.</param>
		internal RichAppResponse(Uri clientCallback, Version version)
			: base(version, MessageTransport.Indirect, clientCallback) {
		}

		/// <summary>
		/// Gets or sets the verification code.
		/// </summary>
		/// <value>
		/// The long-lived credential assigned by the Authorization Server to this Client for
		/// use in accessing the authorizing user's protected resources.
		/// </value>
		[MessagePart(Protocol.wrap_verification_code, IsRequired = true, AllowEmpty = false)]
		internal string VerificationCode { get; set; }

		/// <summary>
		/// Gets or sets state of the client that should be sent back with the authorization response.
		/// </summary>
		/// <value>
		/// An opaque value that Clients can use to maintain state associated with this request. 
		/// </value>
		/// <remarks>
		/// This parameter is required if the Client included it in <see cref="RichAppRequest.ClientState"/>.
		/// </remarks>
		[MessagePart(Protocol.wrap_client_state, IsRequired = false, AllowEmpty = true)]
		internal string ClientState { get; set; }

		/// <summary>
		/// Gets a value indicating whether the user granted the authorization request.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if authorization is granted; otherwise, <c>false</c>.
		/// </value>
		internal bool IsGranted {
			get { return !string.IsNullOrEmpty(this.VerificationCode) && this.VerificationCode != Protocol.user_denied; }
		}
	}
}
