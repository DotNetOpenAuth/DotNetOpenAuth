//-----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationSuccessResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// The message sent by the Authorization Server to the Client via the user agent
	/// to indicate that user authorization was granted, and to return the user
	/// to the Client where they started their experience.
	/// </summary>
	internal class EndUserAuthorizationSuccessResponse : MessageBase, IMessageWithClientState, ITokenCarryingRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationSuccessResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The URL to redirect to so the client receives the message. This may not be built into the request message if the client pre-registered the URL with the authorization server.</param>
		/// <param name="version">The protocol version.</param>
		internal EndUserAuthorizationSuccessResponse(Uri clientCallback, Version version)
			: base(version, MessageTransport.Indirect, clientCallback) {
			Contract.Requires<ArgumentNullException>(version != null);
			Contract.Requires<ArgumentNullException>(clientCallback != null);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationSuccessResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The URL to redirect to so the client receives the message. This may not be built into the request message if the client pre-registered the URL with the authorization server.</param>
		/// <param name="request">The authorization request from the user agent on behalf of the client.</param>
		internal EndUserAuthorizationSuccessResponse(Uri clientCallback, EndUserAuthorizationRequest request)
			: base(request, clientCallback) {
			Contract.Requires<ArgumentNullException>(clientCallback != null, "clientCallback");
			Contract.Requires<ArgumentNullException>(request != null, "request");
			((IMessageWithClientState)this).ClientState = request.ClientState;
		}

		[MessagePart(Protocol.code, AllowEmpty = false, IsRequired = true)] // TODO: this isn't required when the access_token part is present.
		internal string AuthorizationCode { get; set; }

		[MessagePart(Protocol.access_token, AllowEmpty = false, IsRequired = false)]
		internal string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets some state as provided by the client in the authorization request.
		/// </summary>
		/// <value>An opaque value defined by the client.</value>
		/// <remarks>
		/// REQUIRED if the Client sent the value in the <see cref="EndUserAuthorizationRequest"/>.
		/// </remarks>
		[MessagePart(Protocol.state, IsRequired = false, AllowEmpty = true)]
		string IMessageWithClientState.ClientState { get; set; }

		/// <summary>
		/// Gets or sets the lifetime of the authorization.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart(Protocol.expires_in, IsRequired = false, Encoder = typeof(TimespanSecondsEncoder))]
		internal TimeSpan? Lifetime { get; set; }

		/// <summary>
		/// Gets or sets the scope.
		/// </summary>
		/// <value>The scope.</value>
		[MessagePart(Protocol.scope, IsRequired = false, AllowEmpty = true)]
		public string Scope { get; set; }

		/// <summary>
		/// Gets or sets the authorizing user's account name.
		/// </summary>
		internal string AuthorizingUsername { get; set; }

		#region ITokenCarryingRequest Members

		string ITokenCarryingRequest.CodeOrToken {
			get { return this.AuthorizationCode; }
			set { this.AuthorizationCode = value;}
		}

		CodeOrTokenType ITokenCarryingRequest.CodeOrTokenType {
			get { return CodeOrTokenType.AuthorizationCode; }
		}

		IAuthorizationDescription ITokenCarryingRequest.AuthorizationDescription { get; set; }

		#endregion

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
		protected override void EnsureValidMessage()
		{
			base.EnsureValidMessage();

			ErrorUtilities.VerifyProtocol(
				!string.IsNullOrEmpty(this.AuthorizationCode) || !string.IsNullOrEmpty(this.AccessToken),
				MessagingStrings.RequiredParametersMissing,
				this.GetType().Name,
				string.Join(", ", new string[] { Protocol.code,Protocol.access_token}));
		}
	}
}
