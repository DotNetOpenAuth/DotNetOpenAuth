//-----------------------------------------------------------------------
// <copyright file="OAuthPrincipal.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Security.Principal;

	/// <summary>
	/// Represents an OAuth consumer that is impersonating a known user on the system.
	/// </summary>
	[SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable", Justification = "Not cocreatable.")]
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
		/// <param name="userName">The username.</param>
		/// <param name="roles">The roles this user belongs to.</param>
		public OAuthPrincipal(string userName, string[] roles)
			: this(new OAuthIdentity(userName), roles) {
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
		/// Gets or sets the access token used to create this principal.
		/// </summary>
		/// <value>A non-empty string.</value>
		public string AccessToken { get; protected set; }

		/// <summary>
		/// Gets the roles that this principal has as a ReadOnlyCollection.
		/// </summary>
		public ReadOnlyCollection<string> Roles
		{
			get { return new ReadOnlyCollection<string>(this.roles.ToList()); }
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
		/// <remarks>
		/// The role membership check uses <see cref="StringComparer.OrdinalIgnoreCase"/>.
		/// </remarks>
		public bool IsInRole(string role) {
			return this.roles.Contains(role, StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		/// <summary>
		/// Creates a new instance of GenericPrincipal based on this OAuthPrincipal.
		/// </summary>
		/// <returns>A new instance of GenericPrincipal with a GenericIdentity, having the same username and roles as this OAuthPrincipal and OAuthIdentity</returns>
		public GenericPrincipal CreateGenericPrincipal()
		{
			return new GenericPrincipal(new GenericIdentity(this.Identity.Name), this.roles.ToArray());
		}
	}
}
