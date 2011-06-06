//-----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationSuccessAccessTokenResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// The message sent by the Authorization Server to the Client via the user agent
	/// to indicate that user authorization was granted, carrying only an access token,
	/// and to return the user to the Client where they started their experience.
	/// </summary>
	internal class EndUserAuthorizationSuccessAccessTokenResponse : EndUserAuthorizationSuccessResponseBase, ITokenCarryingRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationSuccessAccessTokenResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The URL to redirect to so the client receives the message. This may not be built into the request message if the client pre-registered the URL with the authorization server.</param>
		/// <param name="version">The protocol version.</param>
		internal EndUserAuthorizationSuccessAccessTokenResponse(Uri clientCallback, Version version)
			: base(clientCallback, version) {
			Contract.Requires<ArgumentNullException>(version != null);
			Contract.Requires<ArgumentNullException>(clientCallback != null);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationSuccessAccessTokenResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The URL to redirect to so the client receives the message. This may not be built into the request message if the client pre-registered the URL with the authorization server.</param>
		/// <param name="request">The authorization request from the user agent on behalf of the client.</param>
		internal EndUserAuthorizationSuccessAccessTokenResponse(Uri clientCallback, EndUserAuthorizationRequest request)
			: base(clientCallback, request) {
			Contract.Requires<ArgumentNullException>(clientCallback != null);
			Contract.Requires<ArgumentNullException>(request != null);
			((IMessageWithClientState)this).ClientState = request.ClientState;
		}

		#region ITokenCarryingRequest Members

		/// <summary>
		/// Gets or sets the verification code or refresh/access token.
		/// </summary>
		/// <value>The code or token.</value>
		string ITokenCarryingRequest.CodeOrToken {
			get { return this.AccessToken; }
			set { this.AccessToken = value; }
		}

		/// <summary>
		/// Gets the type of the code or token.
		/// </summary>
		/// <value>The type of the code or token.</value>
		CodeOrTokenType ITokenCarryingRequest.CodeOrTokenType {
			get { return CodeOrTokenType.AccessToken; }
		}

		/// <summary>
		/// Gets or sets the authorization that the token describes.
		/// </summary>
		/// <value></value>
		IAuthorizationDescription ITokenCarryingRequest.AuthorizationDescription { get; set; }

		#endregion

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>The access token.</value>
		[MessagePart(Protocol.access_token, IsRequired = true)]
		internal string AccessToken { get; set; }
	}
}
