//-----------------------------------------------------------------------
// <copyright file="OAuthPrincipal.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Security.Principal;

	/// <summary>
	/// Represents an OAuth consumer that is impersonating a known user on the system.
	/// </summary>
	[Serializable]
	[ComVisible(true)]
	public class OAuthPrincipal : IPrincipal {
		/// <summary>
		/// The roles this user belongs to.
		/// </summary>
		private ICollection<string> roles;

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthPrincipal"/> class.
		/// </summary>
		/// <param name="token">The access token.</param>
		internal OAuthPrincipal(IServiceProviderAccessToken token)
			: this(token.Username, token.Roles) {
			Contract.Requires(token != null);

			this.AccessToken = token.Token;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthPrincipal"/> class.
		/// </summary>
		/// <param name="identity">The identity.</param>
		/// <param name="roles">The roles this user belongs to.</param>
		internal OAuthPrincipal(OAuthIdentity identity, string[] roles) {
			this.Identity = identity;
			this.roles = roles;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthPrincipal"/> class.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <param name="roles">The roles this user belongs to.</param>
		internal OAuthPrincipal(string username, string[] roles)
			: this(new OAuthIdentity(username), roles) {
		}

		/// <summary>
		/// Gets the access token used to create this principal.
		/// </summary>
		/// <value>A non-empty string.</value>
		public string AccessToken { get; private set; }

		#region IPrincipal Members

		/// <summary>
		/// Gets the identity of the current principal.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The <see cref="T:System.Security.Principal.IIdentity"/> object associated with the current principal.
		/// </returns>
		public IIdentity Identity { get; private set; }

		/// <summary>
		/// Determines whether the current principal belongs to the specified role.
		/// </summary>
		/// <param name="role">The name of the role for which to check membership.</param>
		/// <returns>
		/// true if the current principal is a member of the specified role; otherwise, false.
		/// </returns>
		/// <remarks>
		/// The role membership check uses <see cref="StringComparer.OrdinalIgnoreCase"/>.
		/// </remarks>
		public bool IsInRole(string role) {
			return this.roles.Contains(role, StringComparer.OrdinalIgnoreCase);
		}

		#endregion
	}
}
