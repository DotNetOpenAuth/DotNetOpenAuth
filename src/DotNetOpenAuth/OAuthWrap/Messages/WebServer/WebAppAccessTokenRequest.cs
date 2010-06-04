//-----------------------------------------------------------------------
// <copyright file="WebAppAccessTokenRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Diagnostics.Contracts;
	using ChannelElements;
	using Configuration;
	using Messaging;
	using WebServer;

	/// <summary>
	/// A message sent by the Client directly to the Authorization Server to exchange
	/// the verification code for an Access Token.
	/// </summary>
	/// <remarks>
	/// Used by the Web App (and Rich App?) profiles.
	/// </remarks>
	internal class WebAppAccessTokenRequest : MessageBase, IAccessTokenRequest, ITokenCarryingRequest, IOAuthDirectResponseFormat {
		/// <summary>
		/// The type of message.
		/// </summary>
		[MessagePart(Protocol.type, IsRequired = true)]
		private const string Type = "web_server";

		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppAccessTokenRequest"/> class.
		/// </summary>
		/// <param name="accessTokenEndpoint">The Authorization Server's access token endpoint URL.</param>
		/// <param name="version">The version.</param>
		internal WebAppAccessTokenRequest(Uri accessTokenEndpoint, Version version)
			: base(version, MessageTransport.Direct, accessTokenEndpoint) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebAppAccessTokenRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal WebAppAccessTokenRequest(AuthorizationServerDescription authorizationServer)
			: this(authorizationServer.TokenEndpoint, authorizationServer.Version) {
			Contract.Requires<ArgumentNullException>(authorizationServer != null);
			Contract.Requires<ArgumentException>(authorizationServer.Version != null);
			Contract.Requires<ArgumentException>(authorizationServer.TokenEndpoint != null);

			// We prefer URL encoding of the data.
			this.Format = ResponseFormat.Form;
		}

		CodeOrTokenType ITokenCarryingRequest.CodeOrTokenType {
			get { return CodeOrTokenType.VerificationCode; }
		}

		string ITokenCarryingRequest.CodeOrToken {
			get { return this.VerificationCode; }
			set { this.VerificationCode = value; }
		}

		IAuthorizationDescription ITokenCarryingRequest.AuthorizationDescription { get; set; }

		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		/// <value>The client identifier.</value>
		[MessagePart(Protocol.client_id, IsRequired = true, AllowEmpty = false)]
		public string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the client secret.
		/// </summary>
		/// <value>The client secret.</value>
		/// <remarks>
		/// REQUIRED if the client identifier has a matching secret. The client secret as described in Section 3.4  (Client Credentials). 
		/// </remarks>
		[MessagePart(Protocol.client_secret, IsRequired = false, AllowEmpty = true)]
		public string ClientSecret { get; set; }

		/// <summary>
		/// Gets or sets the verification code previously communicated to the Client
		/// in <see cref="WebAppSuccessResponse.VerificationCode"/>.
		/// </summary>
		/// <value>The verification code received from the authorization server.</value>
		[MessagePart(Protocol.code, IsRequired = true, AllowEmpty = false)]
		internal string VerificationCode { get; set; }

		/// <summary>
		/// Gets or sets the callback URL used in <see cref="WebAppRequest.Callback"/>
		/// </summary>
		/// <value>
		/// The Callback URL used to obtain the Verification Code.
		/// </value>
		[MessagePart(Protocol.redirect_uri, IsRequired = true, AllowEmpty = false)]
		internal Uri Callback { get; set; }

		/// <summary>
		/// Gets or sets the type of the secret.
		/// </summary>
		/// <value>The type of the secret.</value>
		/// <remarks>
		/// OPTIONAL. The access token secret type as described by Section 5.3  (Cryptographic Tokens Requests). If omitted, the authorization server will issue a bearer token (an access token without a matching secret) as described by Section 5.2  (Bearer Token Requests). 
		/// </remarks>
		[MessagePart(Protocol.secret_type, IsRequired = false, AllowEmpty = false)]
		public string SecretType { get; set; }

		ResponseFormat IOAuthDirectResponseFormat.Format {
			get { return this.Format.HasValue ? this.Format.Value : ResponseFormat.Json; }
		}

		[MessagePart(Protocol.format, Encoder = typeof(ResponseFormatEncoder))]
		private ResponseFormat? Format { get; set; }

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
			ErrorUtilities.VerifyProtocol(
				DotNetOpenAuthSection.Configuration.Messaging.RelaxSslRequirements || this.Recipient.IsTransportSecure(),
				OAuthWrapStrings.HttpsRequired);
		}
	}
}
