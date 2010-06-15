//-----------------------------------------------------------------------
// <copyright file="WebServerSuccessResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;

	/// <summary>
	/// The message sent by the Authorization Server to the Client via the user agent
	/// to indicate that user authorization was granted, and to return the user
	/// to the Client where they started their experience.
	/// </summary>
	internal class WebServerSuccessResponse : EndUserAuthorizationSuccessResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebServerSuccessResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The client callback.</param>
		/// <param name="version">The protocol version.</param>
		internal WebServerSuccessResponse(Uri clientCallback, Version version)
			: base(clientCallback, version) {
			Contract.Requires<ArgumentNullException>(version != null);
			Contract.Requires<ArgumentNullException>(clientCallback != null);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebServerSuccessResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The client callback.</param>
		/// <param name="request">The request.</param>
		internal WebServerSuccessResponse(Uri clientCallback, EndUserAuthorizationRequest request)
			: base(clientCallback, request) {
			Contract.Requires<ArgumentNullException>(clientCallback != null, "clientCallback");
			Contract.Requires<ArgumentNullException>(request != null, "request");
			((IMessageWithClientState)this).ClientState = ((IMessageWithClientState)request).ClientState;
		}

		/// <summary>
		/// Gets or sets the verification code.
		/// </summary>
		/// <value>
		/// The long-lived credential assigned by the Authorization Server to this Consumer for
		/// use in accessing the authorizing user's protected resources.
		/// </value>
		[MessagePart(Protocol.code, IsRequired = true, AllowEmpty = false)]
		internal new string VerificationCode {
			get { return base.VerificationCode; }
			set { base.VerificationCode = value; }
		}
	}
}
