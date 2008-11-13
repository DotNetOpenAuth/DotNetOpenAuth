//-----------------------------------------------------------------------
// <copyright file="ProviderSecuritySettings.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	/// <summary>
	/// Security settings that are applicable to providers.
	/// </summary>
	internal sealed class ProviderSecuritySettings : SecuritySettings {
		/// <summary>
		/// Initializes a new instance of the <see cref="ProviderSecuritySettings"/> class.
		/// </summary>
		internal ProviderSecuritySettings()
			: base(true) {
		}

		/// <summary>
		/// Gets or sets a value indicating whether OpenID 1.x relying parties that may not be
		/// protecting their users from replay attacks are protected from
		/// replay attacks by this provider.
		/// *** This property is a placeholder for a feature that has not been written yet. ***
		/// </summary>
		/// <remarks>
		/// <para>Nonces for protection against replay attacks were not mandated
		/// by OpenID 1.x, which leaves users open to replay attacks.</para>
		/// <para>This feature works by preventing associations from being formed
		/// with OpenID 1.x relying parties, thereby forcing them into
		/// "dumb" mode and verifying every claim with this provider.
		/// This gives the provider an opportunity to verify its own nonce
		/// to protect against replay attacks.</para>
		/// </remarks>
		internal bool ProtectDownlevelReplayAttacks { get; set; }
	}
}
