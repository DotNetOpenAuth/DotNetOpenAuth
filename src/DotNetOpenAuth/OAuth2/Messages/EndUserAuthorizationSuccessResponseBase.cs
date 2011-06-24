//-----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationSuccessResponseBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Security.Cryptography;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// The message sent by the Authorization Server to the Client via the user agent
	/// to indicate that user authorization was granted, and to return the user
	/// to the Client where they started their experience.
	/// </summary>
	public abstract class EndUserAuthorizationSuccessResponseBase : MessageBase, IMessageWithClientState {
		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationSuccessResponseBase"/> class.
		/// </summary>
		/// <param name="clientCallback">The URL to redirect to so the client receives the message. This may not be built into the request message if the client pre-registered the URL with the authorization server.</param>
		/// <param name="version">The protocol version.</param>
		internal EndUserAuthorizationSuccessResponseBase(Uri clientCallback, Version version)
			: base(version, MessageTransport.Indirect, clientCallback) {
			Contract.Requires<ArgumentNullException>(version != null);
			Contract.Requires<ArgumentNullException>(clientCallback != null);
			this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationSuccessResponseBase"/> class.
		/// </summary>
		/// <param name="clientCallback">The URL to redirect to so the client receives the message. This may not be built into the request message if the client pre-registered the URL with the authorization server.</param>
		/// <param name="request">The authorization request from the user agent on behalf of the client.</param>
		internal EndUserAuthorizationSuccessResponseBase(Uri clientCallback, EndUserAuthorizationRequest request)
			: base(request, clientCallback) {
			Contract.Requires<ArgumentNullException>(clientCallback != null);
			Contract.Requires<ArgumentNullException>(request != null);
			((IMessageWithClientState)this).ClientState = request.ClientState;
			this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
			this.Scope.ResetContents(request.Scope);
		}

		/// <summary>
		/// Gets or sets some state as provided by the client in the authorization request.
		/// </summary>
		/// <value>An opaque value defined by the client.</value>
		/// <remarks>
		/// REQUIRED if the Client sent the value in the <see cref="EndUserAuthorizationRequest"/>.
		/// </remarks>
		[MessagePart(Protocol.state, IsRequired = false)]
		string IMessageWithClientState.ClientState { get; set; }

		/// <summary>
		/// Gets the scope of the <see cref="AccessToken"/> if one is given; otherwise the scope of the authorization code.
		/// </summary>
		/// <value>The scope.</value>
		public ICollection<string> Scope { get; protected set; }

		/// <summary>
		/// Gets or sets the authorizing user's account name.
		/// </summary>
		internal string AuthorizingUsername { get; set; }
	}
}
