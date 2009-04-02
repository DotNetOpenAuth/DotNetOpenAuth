//-----------------------------------------------------------------------
// <copyright file="WellKnownIssuers.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.InfoCard {
	/// <summary>
	/// Common InfoCard issuers.
	/// </summary>
	public sealed class WellKnownIssuers {
		/// <summary>
		/// The Issuer URI to use for self-issued cards.
		/// </summary>
		public const string SelfIssued = "http://schemas.xmlsoap.org/ws/2005/05/identity/issuer/self";

		/// <summary>
		/// Prevents a default instance of the <see cref="WellKnownIssuers"/> class from being created.
		/// </summary>
		private WellKnownIssuers() {
		}
	}
}
