//-----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationSuccessResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;

	/// <summary>
	/// The message sent by the Authorization Server to the Client via the user agent
	/// to indicate that user authorization was granted, and to return the user
	/// to the Client where they started their experience.
	/// </summary>
	internal class EndUserAuthorizationSuccessResponse : MessageBase, IMessageWithClientState, ITokenCarryingRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationSuccessResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The client callback.</param>
		/// <param name="version">The protocol version.</param>
		internal EndUserAuthorizationSuccessResponse(Uri clientCallback, Version version)
			: base(version, MessageTransport.Indirect, clientCallback) {
			Contract.Requires<ArgumentNullException>(version != null);
			Contract.Requires<ArgumentNullException>(clientCallback != null);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationSuccessResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The client callback.</param>
		/// <param name="request">The request.</param>
		internal EndUserAuthorizationSuccessResponse(Uri clientCallback, EndUserAuthorizationRequest request)
			: base(request, clientCallback) {
			Contract.Requires<ArgumentNullException>(clientCallback != null, "clientCallback");
			Contract.Requires<ArgumentNullException>(request != null, "request");
			((IMessageWithClientState)this).ClientState = ((IMessageWithClientState)request).ClientState;
		}

		string ITokenCarryingRequest.CodeOrToken {
			get { return this.VerificationCode; }
			set { this.VerificationCode = value; }
		}

		CodeOrTokenType ITokenCarryingRequest.CodeOrTokenType {
			get { return CodeOrTokenType.VerificationCode; }
		}

		IAuthorizationDescription ITokenCarryingRequest.AuthorizationDescription { get; set; }

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
		/// Gets or sets the scope.
		/// </summary>
		/// <value>The scope.</value>
		[MessagePart(Protocol.scope, IsRequired = false, AllowEmpty = true)]
		public string Scope { get; set; }

		/// <summary>
		/// Gets or sets the verification code.
		/// </summary>
		/// <value>
		/// The long-lived credential assigned by the Authorization Server to this Consumer for
		/// use in accessing the authorizing user's protected resources.
		/// </value>
		[MessagePart(Protocol.code, IsRequired = false, AllowEmpty = false)]
		internal string VerificationCode { get; set; }

		/// <summary>
		/// Gets or sets the authorizing user's account name.
		/// </summary>
		internal string AuthorizingUsername { get; set; }
	}
}
