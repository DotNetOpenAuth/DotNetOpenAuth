//-----------------------------------------------------------------------
// <copyright file="DeviceAccessTokenRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;

	/// <summary>
	/// A message from the Client to the Authorization Server exchanging a
	/// verification code for refresh and access tokens.
	/// </summary>
	internal class DeviceAccessTokenRequest : MessageBase, IAccessTokenRequest, IOAuthDirectResponseFormat {
		/// <summary>
		/// A constant that identifies the flow this message belongs to.
		/// </summary>
		[MessagePart(Protocol.type, IsRequired = true)]
		private const string MessageType = "device_token";

		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceAccessTokenRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal DeviceAccessTokenRequest(Uri authorizationServer, Version version)
			: base(version, MessageTransport.Direct, authorizationServer) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceAccessTokenRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal DeviceAccessTokenRequest(AuthorizationServerDescription authorizationServer)
			: this(authorizationServer.TokenEndpoint, authorizationServer.Version) {
			Contract.Requires<ArgumentNullException>(authorizationServer != null);
			Contract.Requires<ArgumentException>(authorizationServer.Version != null);
			Contract.Requires<ArgumentException>(authorizationServer.TokenEndpoint != null);

			// We prefer URL encoding of the data.
			this.Format = ResponseFormat.Form;
		}

		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		/// <value>The client identifier.</value>
		[MessagePart(Protocol.client_id, IsRequired = true, AllowEmpty = false)]
		public string ClientIdentifier { get; internal set; }

		/// <summary>
		/// Gets the client secret.
		/// </summary>
		/// <value>The client secret.</value>
		string IAccessTokenRequest.ClientSecret {
			get { return null; }
		}

		/// <summary>
		/// Gets or sets the type of the secret.
		/// </summary>
		/// <value>The type of the secret.</value>
		/// <remarks>
		/// OPTIONAL. The access token secret type as described by Section 5.3  (Cryptographic Tokens Requests). If omitted, the authorization server will issue a bearer token (an access token without a matching secret) as described by Section 5.2  (Bearer Token Requests). 
		/// </remarks>
		[MessagePart(Protocol.secret_type, IsRequired = false, AllowEmpty = false)]
		public string SecretType { get; set; }

		/// <summary>
		/// Gets the format the client is requesting the authorization server should deliver the request in.
		/// </summary>
		/// <value>The format.</value>
		ResponseFormat IOAuthDirectResponseFormat.Format {
			get { return this.Format.HasValue ? this.Format.Value : ResponseFormat.Json; }
		}

		/// <summary>
		/// Gets or sets the verification code previously communicated to the Client
		/// in <see cref="DeviceResponse.VerificationCode"/>.
		/// </summary>
		/// <value>The verification code.</value>
		[MessagePart(Protocol.code, IsRequired = true, AllowEmpty = false)]
		internal string VerificationCode { get; set; }

		/// <summary>
		/// Gets or sets the format the client is requesting the authorization server should deliver the request in.
		/// </summary>
		/// <value>The format.</value>
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
			ErrorUtilities.VerifyProtocol(this.Recipient.IsTransportSecure(), OAuthWrapStrings.HttpsRequired);
		}
	}
}
