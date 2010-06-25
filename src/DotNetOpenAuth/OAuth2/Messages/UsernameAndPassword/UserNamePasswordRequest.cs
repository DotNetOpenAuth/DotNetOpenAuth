//-----------------------------------------------------------------------
// <copyright file="UserNamePasswordRequest.cs" company="Andrew Arnott">
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
	/// A request for a delegation code in exchange for a user's confidential 
	/// username and password.
	/// </summary>
	/// <remarks>
	/// After this request has been sent, the consumer application MUST discard
	/// the confidential user credentials and use the delegation code going forward.
	/// </remarks>
	internal class UserNamePasswordRequest : MessageBase, IAccessTokenRequest, IOAuthDirectResponseFormat {
		/// <summary>
		/// A constant that identifies the flow this request belongs to.
		/// </summary>
		[MessagePart(Protocol.type, IsRequired = true)]
		private const string Type = "username";

		/// <summary>
		/// Initializes a new instance of the <see cref="UserNamePasswordRequest"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal UserNamePasswordRequest(Uri tokenEndpoint, Version version)
			: base(version, MessageTransport.Direct, tokenEndpoint) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UserNamePasswordRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal UserNamePasswordRequest(AuthorizationServerDescription authorizationServer)
			: this(authorizationServer.TokenEndpoint, authorizationServer.Version) {
			Contract.Requires<ArgumentNullException>(authorizationServer != null);
			Contract.Requires<ArgumentException>(authorizationServer.Version != null);
			Contract.Requires<ArgumentException>(authorizationServer.TokenEndpoint != null);

			// We prefer URL encoding of the data.
			this.Format = ResponseFormat.Form;
		}

		/// <summary>
		/// Gets or sets the client identifier previously obtained from the Authorization Server.
		/// </summary>
		/// <value>The client identifier.</value>
		[MessagePart(Protocol.client_id, IsRequired = true, AllowEmpty = false)]
		public string ClientIdentifier { get; internal set; }

		/// <summary>
		/// Gets or sets the client secret.
		/// </summary>
		/// <value>The client secret.</value>
		/// <remarks>
		/// REQUIRED. The client secret as described in Section 3.1  (Client Credentials). OPTIONAL if no client secret was issued. 
		/// </remarks>
		[MessagePart(Protocol.client_secret, IsRequired = false, AllowEmpty = true)]
		public string ClientSecret { get; internal set; }

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
		/// Gets or sets the user's account username.
		/// </summary>
		/// <value>The username on the user's account.</value>
		[MessagePart(Protocol.username, IsRequired = true, AllowEmpty = false)]
		internal string UserName { get; set; }

		/// <summary>
		/// Gets or sets the user's password.
		/// </summary>
		/// <value>The password.</value>
		[MessagePart(Protocol.password, IsRequired = true, AllowEmpty = true)]
		internal string Password { get; set; }

		/// <summary>
		/// Gets or sets the CAPTCHA puzzle that the user just solved, if applicable.
		/// </summary>
		/// <value>The captcha puzzle location.</value>
		[MessagePart(Protocol.wrap_captcha_url, IsRequired = false, AllowEmpty = false)]
		internal Uri CaptchaPuzzle { get; set; }

		/// <summary>
		/// Gets or sets the solution to the CAPTCHA puzzle the user just solved, if applicable.
		/// </summary>
		/// <value>The CAPTCHA solution.</value>
		[MessagePart(Protocol.wrap_captcha_solution, IsRequired = false, AllowEmpty = false)]
		internal string CaptchaSolution { get; set; }

		/// <summary>
		/// Gets or sets the scope.
		/// </summary>
		/// <value>The scope.</value>
		[MessagePart(Protocol.scope, IsRequired = false, AllowEmpty = true)]
		internal string Scope { get; set; }

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
			ErrorUtilities.VerifyProtocol((this.CaptchaPuzzle == null) == (this.CaptchaSolution == null), "CAPTCHA puzzle and solution must either be both absent or both present.");
		}
	}
}
