//-----------------------------------------------------------------------
// <copyright file="WellKnownProviders.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	/// <summary>
	/// Common OpenID Provider Identifiers.
	/// </summary>
	public sealed class WellKnownProviders {
		/// <summary>
		/// The Yahoo OP Identifier.
		/// </summary>
		public static readonly Identifier Yahoo = "https://me.yahoo.com/";

		/// <summary>
		/// The Google OP Identifier.
		/// </summary>
		public static readonly Identifier Google = "https://www.google.com/accounts/o8/id";

		/// <summary>
		/// The MyOpenID OP Identifier.
		/// </summary>
		public static readonly Identifier MyOpenId = "https://www.myopenid.com/";

		/// <summary>
		/// Prevents a default instance of the <see cref="WellKnownProviders"/> class from being created.
		/// </summary>
		private WellKnownProviders() {
		}
	}
}
