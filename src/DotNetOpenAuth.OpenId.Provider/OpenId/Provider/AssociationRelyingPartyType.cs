//-----------------------------------------------------------------------
// <copyright file="AssociationRelyingPartyType.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	/// <summary>
	/// An enumeration that can specify how a given <see cref="Association"/> is used.
	/// </summary>
	public enum AssociationRelyingPartyType {
		/// <summary>
		/// The <see cref="Association"/> manages a shared secret between
		/// Provider and Relying Party sites that allows the RP to verify
		/// the signature on a message from an OP.
		/// </summary>
		Smart,

		/// <summary>
		/// The <see cref="Association"/> manages a secret known alone by
		/// a Provider that allows the Provider to verify its own signatures
		/// for "dumb" (stateless) relying parties.
		/// </summary>
		Dumb
	}
}
