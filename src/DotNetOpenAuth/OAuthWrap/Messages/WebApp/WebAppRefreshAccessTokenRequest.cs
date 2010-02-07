//-----------------------------------------------------------------------
// <copyright file="WebAppRefreshAccessTokenRequest.cs" company="Andrew Arnott">
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
	/// A request from the Client to the Authorization Server to obtain 
	/// a new Access Token using a Refresh Token.
	/// </summary>
	internal class WebAppRefreshAccessTokenRequest : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppRefreshAccessTokenRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal WebAppRefreshAccessTokenRequest(Uri authorizationServer, Version version)
			: base(version, Messaging.MessageTransport.Direct, authorizationServer) {
				this.HttpMethods = Messaging.HttpDeliveryMethods.PostRequest;
		}

		/// <summary>
		/// Gets or sets the client identifier previously obtained from the Authorization Server.
		/// </summary>
		/// <value>The client identifier.</value>
		[MessagePart(Protocol.wrap_client_id, IsRequired = true, AllowEmpty = false)]
		internal string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the client secret previously obtained from the Authorization Server.
		/// </summary>
		/// <value>The client secret.</value>
		[MessagePart(Protocol.wrap_client_secret, IsRequired = true, AllowEmpty = false)]
		internal string ClientSecret { get; set; }

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
