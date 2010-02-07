//-----------------------------------------------------------------------
// <copyright file="UserNamePasswordRefreshAccessTokenRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A request from the client to the authorization server for a new access token
	/// after a previously supplied access token has expired.
	/// </summary>
	internal class UserNamePasswordRefreshAccessTokenRequest : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserNamePasswordRefreshAccessTokenRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal UserNamePasswordRefreshAccessTokenRequest(Uri authorizationServer, Version version)
			: base(version, MessageTransport.Direct, authorizationServer) {
				this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		/// <summary>
		/// Gets or sets the refresh token that was received in
		/// <see cref="UserNamePasswordSuccessResponse.RefreshToken"/>.
		/// </summary>
		/// <value>The refresh token.</value>
		[MessagePart(Protocol.wrap_refresh_token, IsRequired = true, AllowEmpty = false)]
		internal string RefreshToken { get; set; }

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// 	<para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the
		/// message to see if it conforms to the protocol.</para>
		/// 	<para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();
			ErrorUtilities.VerifyProtocol(this.Recipient.IsTransportSecure(), SimpleAuthStrings.HttpsRequired);
		}
	}
}
