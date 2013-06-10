//-----------------------------------------------------------------------
// <copyright file="OpenIdProviderSecuritySettingsElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// Represents the .config file element that allows for setting the security policies of the Provider.
	/// </summary>
	internal class OpenIdProviderSecuritySettingsElement : ConfigurationElement {
		/// <summary>
		/// Gets the name of the @protectDownlevelReplayAttacks attribute.
		/// </summary>
		private const string ProtectDownlevelReplayAttacksConfigName = "protectDownlevelReplayAttacks";

		/// <summary>
		/// Gets the name of the @minimumHashBitLength attribute.
		/// </summary>
		private const string MinimumHashBitLengthConfigName = "minimumHashBitLength";

		/// <summary>
		/// Gets the name of the @maximumHashBitLength attribute.
		/// </summary>
		private const string MaximumHashBitLengthConfigName = "maximumHashBitLength";

		/// <summary>
		/// The name of the associations collection sub-element.
		/// </summary>
		private const string AssociationsConfigName = "associations";

		/// <summary>
		/// The name of the @encodeAssociationSecretsInHandles attribute.
		/// </summary>
		private const string EncodeAssociationSecretsInHandlesConfigName = "encodeAssociationSecretsInHandles";

		/// <summary>
		/// Gets the name of the @requireSsl attribute.
		/// </summary>
		private const string RequireSslConfigName = "requireSsl";

		/// <summary>
		/// Gets the name of the @unsolicitedAssertionVerification attribute.
		/// </summary>
		private const string UnsolicitedAssertionVerificationConfigName = "unsolicitedAssertionVerification";

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProviderSecuritySettingsElement"/> class.
		/// </summary>
		public OpenIdProviderSecuritySettingsElement() {
		}

		/// <summary>
		/// Gets or sets a value indicating whether all discovery and authentication should require SSL security.
		/// </summary>
		[ConfigurationProperty(RequireSslConfigName, DefaultValue = false)]
		public bool RequireSsl {
			get { return (bool)this[RequireSslConfigName]; }
			set { this[RequireSslConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the minimum length of the hash that protects the protocol from hijackers.
		/// </summary>
		[ConfigurationProperty(MinimumHashBitLengthConfigName, DefaultValue = SecuritySettings.MinimumHashBitLengthDefault)]
		public int MinimumHashBitLength {
			get { return (int)this[MinimumHashBitLengthConfigName]; }
			set { this[MinimumHashBitLengthConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the maximum length of the hash that protects the protocol from hijackers.
		/// </summary>
		[ConfigurationProperty(MaximumHashBitLengthConfigName, DefaultValue = SecuritySettings.MaximumHashBitLengthRPDefault)]
		public int MaximumHashBitLength {
			get { return (int)this[MaximumHashBitLengthConfigName]; }
			set { this[MaximumHashBitLengthConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the Provider should take special care 
		/// to protect OpenID 1.x relying parties against replay attacks.
		/// </summary>
		[ConfigurationProperty(ProtectDownlevelReplayAttacksConfigName, DefaultValue = ProviderSecuritySettings.ProtectDownlevelReplayAttacksDefault)]
		public bool ProtectDownlevelReplayAttacks {
			get { return (bool)this[ProtectDownlevelReplayAttacksConfigName]; }
			set { this[ProtectDownlevelReplayAttacksConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the level of verification a Provider performs on an identifier before
		/// sending an unsolicited assertion for it.
		/// </summary>
		/// <value>The default value is <see cref="ProviderSecuritySettings.UnsolicitedAssertionVerificationLevel.RequireSuccess"/>.</value>
		[ConfigurationProperty(UnsolicitedAssertionVerificationConfigName, DefaultValue = ProviderSecuritySettings.UnsolicitedAssertionVerificationDefault)]
		public ProviderSecuritySettings.UnsolicitedAssertionVerificationLevel UnsolicitedAssertionVerification {
			get { return (ProviderSecuritySettings.UnsolicitedAssertionVerificationLevel)this[UnsolicitedAssertionVerificationConfigName]; }
			set { this[UnsolicitedAssertionVerificationConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the configured lifetimes of the various association types.
		/// </summary>
		[ConfigurationProperty(AssociationsConfigName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(AssociationTypeCollection))]
		public AssociationTypeCollection AssociationLifetimes {
			get {
				return (AssociationTypeCollection)this[AssociationsConfigName] ?? new AssociationTypeCollection();
			}

			set {
				this[AssociationsConfigName] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the Provider should ease the burden of storing associations
		/// by encoding their secrets (in signed, encrypted form) into the association handles themselves, storing only
		/// a few rotating, private symmetric keys in the Provider's store instead.
		/// </summary>
		[ConfigurationProperty(EncodeAssociationSecretsInHandlesConfigName, DefaultValue = ProviderSecuritySettings.EncodeAssociationSecretsInHandlesDefault)]
		public bool EncodeAssociationSecretsInHandles {
			get { return (bool)this[EncodeAssociationSecretsInHandlesConfigName]; }
			set { this[EncodeAssociationSecretsInHandlesConfigName] = value; }
		}

		/// <summary>
		/// Initializes a programmatically manipulatable bag of these security settings with the settings from the config file.
		/// </summary>
		/// <returns>The newly created security settings object.</returns>
		public ProviderSecuritySettings CreateSecuritySettings() {
			ProviderSecuritySettings settings = new ProviderSecuritySettings();
			settings.RequireSsl = this.RequireSsl;
			settings.MinimumHashBitLength = this.MinimumHashBitLength;
			settings.MaximumHashBitLength = this.MaximumHashBitLength;
			settings.ProtectDownlevelReplayAttacks = this.ProtectDownlevelReplayAttacks;
			settings.UnsolicitedAssertionVerification = this.UnsolicitedAssertionVerification;
			settings.EncodeAssociationSecretsInHandles = this.EncodeAssociationSecretsInHandles;
			foreach (AssociationTypeElement element in this.AssociationLifetimes) {
				Assumes.True(element != null);
				settings.AssociationLifetimes.Add(element.AssociationType, element.MaximumLifetime);
			}

			return settings;
		}
	}
}
