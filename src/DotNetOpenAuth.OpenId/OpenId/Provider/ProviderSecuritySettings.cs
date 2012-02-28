//-----------------------------------------------------------------------
// <copyright file="ProviderSecuritySettings.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Security settings that are applicable to providers.
	/// </summary>
	[Serializable]
	public sealed class ProviderSecuritySettings : SecuritySettings {
		/// <summary>
		/// The default value for the <see cref="ProtectDownlevelReplayAttacks"/> property.
		/// </summary>
		internal const bool ProtectDownlevelReplayAttacksDefault = true;

		/// <summary>
		/// The default value for the <see cref="EncodeAssociationSecretsInHandles"/> property.
		/// </summary>
		internal const bool EncodeAssociationSecretsInHandlesDefault = true;

		/// <summary>
		/// The default value for the <see cref="SignOutgoingExtensions"/> property.
		/// </summary>
		internal const bool SignOutgoingExtensionsDefault = true;

		/// <summary>
		/// The default value for the <see cref="UnsolicitedAssertionVerification"/> property.
		/// </summary>
		internal const UnsolicitedAssertionVerificationLevel UnsolicitedAssertionVerificationDefault = UnsolicitedAssertionVerificationLevel.RequireSuccess;

		/// <summary>
		/// The subset of association types and their customized lifetimes.
		/// </summary>
		private IDictionary<string, TimeSpan> associationLifetimes = new Dictionary<string, TimeSpan>();

		/// <summary>
		/// Initializes a new instance of the <see cref="ProviderSecuritySettings"/> class.
		/// </summary>
		internal ProviderSecuritySettings()
			: base(true) {
			this.SignOutgoingExtensions = SignOutgoingExtensionsDefault;
			this.ProtectDownlevelReplayAttacks = ProtectDownlevelReplayAttacksDefault;
			this.UnsolicitedAssertionVerification = UnsolicitedAssertionVerificationDefault;
		}

		/// <summary>
		/// The behavior a Provider takes when verifying that it is authoritative for an
		/// identifier it is about to send an unsolicited assertion for.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "By design")]
		public enum UnsolicitedAssertionVerificationLevel {
			/// <summary>
			/// Always verify that the Provider is authoritative for an identifier before
			/// sending an unsolicited assertion for it and fail if it is not.
			/// </summary>
			RequireSuccess,

			/// <summary>
			/// Always check that the Provider is authoritative for an identifier before
			/// sending an unsolicited assertion for it, but only log failures, and proceed
			/// to send the unsolicited assertion.
			/// </summary>
			LogWarningOnFailure,

			/// <summary>
			/// Never verify that the Provider is authoritative for an identifier before
			/// sending an unsolicited assertion for it.
			/// </summary>
			/// <remarks>
			/// This setting is useful for web servers that refuse to allow a Provider to
			/// introspectively perform an HTTP GET on itself, when sending unsolicited assertions
			/// for identifiers that the OP controls.
			/// </remarks>
			NeverVerify,
		}

		/// <summary>
		/// Gets a subset of the available association types and their
		/// customized maximum lifetimes.
		/// </summary>
		public IDictionary<string, TimeSpan> AssociationLifetimes {
			get { return this.associationLifetimes; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether Relying Party discovery will only
		/// succeed if done over a secure HTTPS channel.
		/// </summary>
		/// <value>Default is <c>false</c>.</value>
		public bool RequireSsl { get; set; }

		/// <summary>
		/// Gets or sets the level of verification a Provider performs on an identifier before
		/// sending an unsolicited assertion for it.
		/// </summary>
		/// <value>The default value is <see cref="UnsolicitedAssertionVerificationLevel.RequireSuccess"/>.</value>
		public UnsolicitedAssertionVerificationLevel UnsolicitedAssertionVerification { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Provider should ease the burden of storing associations
		/// by encoding them in signed, encrypted form into the association handles themselves, storing only
		/// a few rotating, private symmetric keys in the Provider's store instead.
		/// </summary>
		/// <value>The default value for this property is <c>true</c>.</value>
		public bool EncodeAssociationSecretsInHandles { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether OpenID 1.x relying parties that may not be
		/// protecting their users from replay attacks are protected from
		/// replay attacks by this provider.
		/// </summary>
		/// <value>The default value is <c>true</c>.</value>
		/// <remarks>
		/// <para>Nonces for protection against replay attacks were not mandated
		/// by OpenID 1.x, which leaves users open to replay attacks.</para>
		/// <para>This feature works by preventing associations from being used
		/// with OpenID 1.x relying parties, thereby forcing them into
		/// "dumb" mode and verifying every claim with this provider.
		/// This gives the provider an opportunity to verify its own nonce
		/// to protect against replay attacks.</para>
		/// </remarks>
		internal bool ProtectDownlevelReplayAttacks { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether outgoing extensions are always signed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if outgoing extensions should be signed; otherwise, <c>false</c>.
		/// 	The default is <c>true</c>.
		/// </value>
		/// <remarks>
		/// This property is internal because Providers should never turn it off, but it is
		/// needed for testing the RP's rejection of unsigned extensions.
		/// </remarks>
		internal bool SignOutgoingExtensions { get; set; }

		/// <summary>
		/// Creates a deep clone of this instance.
		/// </summary>
		/// <returns>A new instance that is a deep clone of this instance.</returns>
		internal ProviderSecuritySettings Clone() {
			var securitySettings = new ProviderSecuritySettings();
			foreach (var pair in this.AssociationLifetimes) {
				securitySettings.AssociationLifetimes.Add(pair);
			}

			securitySettings.MaximumHashBitLength = this.MaximumHashBitLength;
			securitySettings.MinimumHashBitLength = this.MinimumHashBitLength;
			securitySettings.ProtectDownlevelReplayAttacks = this.ProtectDownlevelReplayAttacks;
			securitySettings.RequireSsl = this.RequireSsl;
			securitySettings.SignOutgoingExtensions = this.SignOutgoingExtensions;
			securitySettings.UnsolicitedAssertionVerification = this.UnsolicitedAssertionVerification;

			return securitySettings;
		}
	}
}
