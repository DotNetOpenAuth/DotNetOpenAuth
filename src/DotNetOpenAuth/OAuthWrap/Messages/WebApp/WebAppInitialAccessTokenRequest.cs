//-----------------------------------------------------------------------
// <copyright file="WebAppInitialAccessTokenRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;

	/// <summary>
	/// A message sent by the Client directly to the Authorization Server to exchange
	/// the verification code for an Access Token.
	/// </summary>
	/// <remarks>
	/// Used by the Web App (and Rich App?) profiles.
	/// </remarks>
	internal class WebAppInitialAccessTokenRequest : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppInitialAccessTokenRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The token issuer.</param>
		/// <param name="version">The version.</param>
		internal WebAppInitialAccessTokenRequest(Uri authorizationServer, Version version)
			: base(version, MessageTransport.Direct, authorizationServer) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		/// <value>The client identifier.</value>
		[MessagePart(Protocol.wrap_client_id, IsRequired = true, AllowEmpty = false)]
		internal string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the client secret.
		/// </summary>
		/// <value>The client secret.</value>
		[MessagePart(Protocol.wrap_client_secret, IsRequired = true, AllowEmpty = false)]
		internal string ClientSecret { get; set; }

		/// <summary>
		/// Gets or sets the verification code previously communicated to the Client
		/// in <see cref="WebAppSuccessResponse.VerificationCode"/>.
		/// </summary>
		/// <value>The verification code.</value>
		[MessagePart(Protocol.wrap_verification_code, IsRequired = true, AllowEmpty = false)]
		internal string VerificationCode { get; set; }

		/// <summary>
		/// Gets or sets the callback URL used in <see cref="WebAppRequest.Callback"/>
		/// </summary>
		/// <value>
		/// The Callback URL used to obtain the Verification Code.
		/// </value>
		[MessagePart(Protocol.wrap_callback, IsRequired = true, AllowEmpty = false)]
		internal Uri Callback { get; set; }

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
