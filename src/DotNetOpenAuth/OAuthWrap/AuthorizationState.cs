//-----------------------------------------------------------------------
// <copyright file="AuthorizationState.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;

	/// <summary>
	/// A simple memory-only copy of an authorization state.
	/// </summary>
	public class AuthorizationState : IAuthorizationState {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationState"/> class.
		/// </summary>
		public AuthorizationState() {
		}

		public Uri Callback { get; set; }

		public string RefreshToken { get; set; }

		public string AccessToken { get; set; }

		public string AccessTokenSecret { get; set; }

		public string AccessTokenSecretType { get; set; }

		public DateTime? AccessTokenExpirationUtc { get; set; }

		public DateTime? AccessTokenIssueDateUtc { get; set; }

		public string Scope { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is deleted.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is deleted; otherwise, <c>false</c>.
		/// </value>
		public bool IsDeleted { get; set; }

		public virtual void Delete() {
			this.IsDeleted = true;
		}

		public virtual void SaveChanges() {
		}
	}
}