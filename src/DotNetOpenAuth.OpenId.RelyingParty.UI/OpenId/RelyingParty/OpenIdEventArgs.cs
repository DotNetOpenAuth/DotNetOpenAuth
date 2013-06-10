//-----------------------------------------------------------------------
// <copyright file="OpenIdEventArgs.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// The event details passed to event handlers.
	/// </summary>
	public class OpenIdEventArgs : EventArgs {
		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdEventArgs"/> class
		/// with minimal information of an incomplete or failed authentication attempt.
		/// </summary>
		/// <param name="request">The outgoing authentication request.</param>
		internal OpenIdEventArgs(IAuthenticationRequest request) {
			Requires.NotNull(request, "request");

			this.Request = request;
			this.ClaimedIdentifier = request.ClaimedIdentifier;
			this.IsDirectedIdentity = request.IsDirectedIdentity;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdEventArgs"/> class
		/// with information on a completed authentication attempt
		/// (whether that attempt was successful or not).
		/// </summary>
		/// <param name="response">The incoming authentication response.</param>
		internal OpenIdEventArgs(IAuthenticationResponse response) {
			Requires.NotNull(response, "response");

			this.Response = response;
			this.ClaimedIdentifier = response.ClaimedIdentifier;
		}

		/// <summary>
		/// Gets or sets a value indicating whether to cancel
		/// the OpenID authentication and/or login process.
		/// </summary>
		public bool Cancel { get; set; }

		/// <summary>
		/// Gets the Identifier the user is claiming to own.  Or null if the user
		/// is using Directed Identity.
		/// </summary>
		public Identifier ClaimedIdentifier { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the user has selected to let his Provider determine 
		/// the ClaimedIdentifier to use as part of successful authentication.
		/// </summary>
		public bool IsDirectedIdentity { get; private set; }

		/// <summary>
		/// Gets the details of the OpenID authentication request,
		/// and allows for adding extensions.
		/// </summary>
		public IAuthenticationRequest Request { get; private set; }

		/// <summary>
		/// Gets the details of the OpenID authentication response.
		/// </summary>
		public IAuthenticationResponse Response { get; private set; }
	}
}
