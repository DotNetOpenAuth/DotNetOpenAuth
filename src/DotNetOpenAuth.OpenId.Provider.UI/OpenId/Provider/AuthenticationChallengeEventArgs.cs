//-----------------------------------------------------------------------
// <copyright file="AuthenticationChallengeEventArgs.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;

	/// <summary>
	/// The event arguments that include details of the incoming request.
	/// </summary>
	public class AuthenticationChallengeEventArgs : EventArgs {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationChallengeEventArgs"/> class.
		/// </summary>
		/// <param name="request">The incoming authentication request.</param>
		internal AuthenticationChallengeEventArgs(IAuthenticationRequest request) {
			this.Request = request;
		}

		/// <summary>
		/// Gets the incoming authentication request.
		/// </summary>
		public IAuthenticationRequest Request { get; internal set; }
	}
}
