//-----------------------------------------------------------------------
// <copyright file="WellKnownIssuers.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.InfoCard {
	/// <summary>
	/// Common InfoCard issuers.
	/// </summary>
	public class WellKnownIssuers {
		/// <summary>
		/// The Issuer URI to use for self-issued cards.
		/// </summary>
		public const string SelfIssued = "http://schemas.xmlsoap.org/ws/2005/05/identity/issuer/self";

		/// <summary>
		/// Initializes a new instance of the <see cref="WellKnownClaimTypes"/> class.
		/// </summary>
		private WellKnownIssuers() {
		}
	}
}
