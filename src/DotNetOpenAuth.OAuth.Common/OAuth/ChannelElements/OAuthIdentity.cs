//-----------------------------------------------------------------------
// <copyright file="OAuthIdentity.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Runtime.InteropServices;
	using System.Security.Principal;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Represents an OAuth consumer that is impersonating a known user on the system.
	/// </summary>
	[SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable", Justification = "Not cocreatable.")]
	[Serializable]
	[ComVisible(true)]
	public class OAuthIdentity : IIdentity {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthIdentity"/> class.
		/// </summary>
		/// <param name="username">The username.</param>
		internal OAuthIdentity(string username) {
			Requires.NotNullOrEmpty(username, "username");
			this.Name = username;
		}

		#region IIdentity Members

		/// <summary>
		/// Gets the type of authentication used.
		/// </summary>
		/// <value>The constant "OAuth"</value>
		/// <returns>
		/// The type of authentication used to identify the user.
		/// </returns>
		public string AuthenticationType {
			get { return "OAuth"; }
		}

		/// <summary>
		/// Gets a value indicating whether the user has been authenticated.
		/// </summary>
		/// <value>The value <c>true</c></value>
		/// <returns>true if the user was authenticated; otherwise, false.
		/// </returns>
		public bool IsAuthenticated {
			get { return true; }
		}

		/// <summary>
		/// Gets the name of the user who authorized the OAuth token the consumer is using for authorization.
		/// </summary>
		/// <returns>
		/// The name of the user on whose behalf the code is running.
		/// </returns>
		public string Name { get; private set; }

		#endregion
	}
}
