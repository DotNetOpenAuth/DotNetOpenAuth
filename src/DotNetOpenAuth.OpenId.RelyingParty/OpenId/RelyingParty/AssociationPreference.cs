//-----------------------------------------------------------------------
// <copyright file="AssociationPreference.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// Preferences regarding creation and use of an association between a relying party
	/// and provider for authentication.
	/// </summary>
	internal enum AssociationPreference {
		/// <summary>
		/// Indicates that an association should be created for use in authentication
		/// if one has not already been established between the relying party and the
		/// selected provider.
		/// </summary>
		/// <remarks>
		/// Even with this value, if an association attempt fails or the relying party
		/// has no application store to recall associations, the authentication may 
		/// proceed without an association.
		/// </remarks>
		IfPossible,

		/// <summary>
		/// Indicates that an association should be used for authentication only if
		/// it happens to already exist.
		/// </summary>
		IfAlreadyEstablished,

		/// <summary>
		/// Indicates that an authentication attempt should NOT use an OpenID association
		/// between the relying party and the provider, even if an association was previously
		/// created.
		/// </summary>
		Never,
	}
}
