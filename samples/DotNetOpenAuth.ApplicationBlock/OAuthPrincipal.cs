//-----------------------------------------------------------------------
// <copyright file="OAuthPrincipal.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Security.Principal;

	/// <summary>
	/// Represents an OAuth consumer that is impersonating a known user on the system.
	/// </summary>
	[Serializable]
	[ComVisible(true)]
	internal class OAuthPrincipal : IPrincipal {
		/// <summary>
		/// The roles this user belongs to.
		/// </summary>
		private string[] roles;

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
		public bool IsInRole(string role) {
			return this.roles.Contains(role);
		}

		#endregion
	}
}
