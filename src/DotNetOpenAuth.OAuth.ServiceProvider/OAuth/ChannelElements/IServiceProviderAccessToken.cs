//-----------------------------------------------------------------------
// <copyright file="IServiceProviderAccessToken.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A description of an access token and its metadata as required by a Service Provider.
	/// </summary>
	public interface IServiceProviderAccessToken {
		/// <summary>
		/// Gets the token itself.
		/// </summary>
		string Token { get; }

		/// <summary>
		/// Gets the expiration date (local time) for the access token.
		/// </summary>
		/// <value>The expiration date, or <c>null</c> if there is no expiration date.</value>
		DateTime? ExpirationDate { get; }

		/// <summary>
		/// Gets the username of the principal that will be impersonated by this access token.
		/// </summary>
		/// <value>
		/// The name of the user who authorized the OAuth request token originally.
		/// </value>
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Username", Justification = "Breaking change.")]
		string Username { get; }

		/// <summary>
		/// Gets the roles that the OAuth principal should belong to.
		/// </summary>
		/// <value>
		/// The roles that the user belongs to, or a subset of these according to the rights
		/// granted when the user authorized the request token.
		/// </value>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "By design.")]
		string[] Roles { get; }
	}
}
