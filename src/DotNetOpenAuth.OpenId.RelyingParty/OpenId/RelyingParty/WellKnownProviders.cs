//-----------------------------------------------------------------------
// <copyright file="WellKnownProviders.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Common OpenID Provider Identifiers.
	/// </summary>
	public sealed class WellKnownProviders {
		/// <summary>
		/// The Yahoo OP Identifier.
		/// </summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Immutable type")]
		public static readonly Identifier Yahoo = "https://me.yahoo.com/";

		/// <summary>
		/// The Google OP Identifier.
		/// </summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Immutable type")]
		public static readonly Identifier Google = "https://www.google.com/accounts/o8/id";

		/// <summary>
		/// The MyOpenID OP Identifier.
		/// </summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Immutable type")]
		public static readonly Identifier MyOpenId = "https://www.myopenid.com/";

		/// <summary>
		/// The Verisign OP Identifier.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Verisign", Justification = "The spelling is correct.")]
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Immutable type")]
		public static readonly Identifier Verisign = "https://pip.verisignlabs.com/";

		/// <summary>
		/// The MyVidoop OP Identifier.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vidoop", Justification = "The spelling is correct.")]
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Immutable type")]
		public static readonly Identifier MyVidoop = "https://myvidoop.com/";

		/// <summary>
		/// Prevents a default instance of the <see cref="WellKnownProviders"/> class from being created.
		/// </summary>
		private WellKnownProviders() {
		}
	}
}
