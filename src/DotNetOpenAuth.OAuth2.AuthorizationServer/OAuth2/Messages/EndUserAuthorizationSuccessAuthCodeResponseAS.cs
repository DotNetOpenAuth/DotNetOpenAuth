//-----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationSuccessAuthCodeResponseAS.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using Validation;

	/// <summary>
	/// The message sent by the Authorization Server to the Client via the user agent
	/// to indicate that user authorization was granted, carrying an authorization code and possibly an access token,
	/// and to return the user to the Client where they started their experience.
	/// </summary>
	internal class EndUserAuthorizationSuccessAuthCodeResponseAS : EndUserAuthorizationSuccessAuthCodeResponse, IAuthorizationCodeCarryingRequest {
			/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationSuccessAuthCodeResponseAS"/> class.
		/// </summary>
		/// <param name="clientCallback">The URL to redirect to so the client receives the message. This may not be built into the request message if the client pre-registered the URL with the authorization server.</param>
		/// <param name="version">The protocol version.</param>
		internal EndUserAuthorizationSuccessAuthCodeResponseAS(Uri clientCallback, Version version)
			: base(clientCallback, version) {
			Requires.NotNull(version, "version");
			Requires.NotNull(clientCallback, "clientCallback");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationSuccessAuthCodeResponseAS"/> class.
		/// </summary>
		/// <param name="clientCallback">The URL to redirect to so the client receives the message. This may not be built into the request message if the client pre-registered the URL with the authorization server.</param>
		/// <param name="request">The authorization request from the user agent on behalf of the client.</param>
		internal EndUserAuthorizationSuccessAuthCodeResponseAS(Uri clientCallback, EndUserAuthorizationRequest request)
			: base(clientCallback, request) {
			Requires.NotNull(clientCallback, "clientCallback");
			Requires.NotNull(request, "request");
			((IMessageWithClientState)this).ClientState = request.ClientState;
		}

	#region IAuthorizationCodeCarryingRequest Members

		/// <summary>
		/// Gets or sets the authorization code.
		/// </summary>
		string IAuthorizationCodeCarryingRequest.Code {
			get { return this.AuthorizationCode; }
			set { this.AuthorizationCode = value; }
		}

		/// <summary>
		/// Gets or sets the authorization that the token describes.
		/// </summary>
		AuthorizationCode IAuthorizationCodeCarryingRequest.AuthorizationDescription { get; set; }

		/// <summary>
		/// Gets the authorization that the code describes.
		/// </summary>
		IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription {
			get { return ((IAuthorizationCodeCarryingRequest)this).AuthorizationDescription; }
		}

		#endregion
	}
}
