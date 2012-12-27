//-----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationSuccessAuthCodeResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using Validation;

	/// <summary>
	/// The message sent by the Authorization Server to the Client via the user agent
	/// to indicate that user authorization was granted, carrying an authorization code and possibly an access token,
	/// and to return the user to the Client where they started their experience.
	/// </summary>
	internal class EndUserAuthorizationSuccessAuthCodeResponse : EndUserAuthorizationSuccessResponseBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationSuccessAuthCodeResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The URL to redirect to so the client receives the message. This may not be built into the request message if the client pre-registered the URL with the authorization server.</param>
		/// <param name="version">The protocol version.</param>
		internal EndUserAuthorizationSuccessAuthCodeResponse(Uri clientCallback, Version version)
			: base(clientCallback, version) {
			Requires.NotNull(version, "version");
			Requires.NotNull(clientCallback, "clientCallback");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationSuccessAuthCodeResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The URL to redirect to so the client receives the message. This may not be built into the request message if the client pre-registered the URL with the authorization server.</param>
		/// <param name="request">The authorization request from the user agent on behalf of the client.</param>
		internal EndUserAuthorizationSuccessAuthCodeResponse(Uri clientCallback, EndUserAuthorizationRequest request)
			: base(clientCallback, request) {
			Requires.NotNull(clientCallback, "clientCallback");
			Requires.NotNull(request, "request");
			((IMessageWithClientState)this).ClientState = request.ClientState;
		}

		/// <summary>
		/// Gets or sets the authorization code.
		/// </summary>
		/// <value>The authorization code.</value>
		[MessagePart(Protocol.code, IsRequired = true)]
		internal string AuthorizationCode { get; set; }
	}
}
