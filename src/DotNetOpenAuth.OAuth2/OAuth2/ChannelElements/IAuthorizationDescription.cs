//-----------------------------------------------------------------------
// <copyright file="IAuthorizationDescription.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Describes a delegated authorization between a resource server, a client, and a user.
	/// </summary>
	public interface IAuthorizationDescription {
		/// <summary>
		/// Gets the identifier of the client authorized to access protected data.
		/// </summary>
		string ClientIdentifier { get; }

		/// <summary>
		/// Gets the date this authorization was established or the token was issued.
		/// </summary>
		/// <value>A date/time expressed in UTC.</value>
		DateTime UtcIssued { get; }

		/// <summary>
		/// Gets the name on the account whose data on the resource server is accessible using this authorization.
		/// </summary>
		string User { get; }

		/// <summary>
		/// Gets the scope of operations the client is allowed to invoke.
		/// </summary>
		HashSet<string> Scope { get; }
	}
}
