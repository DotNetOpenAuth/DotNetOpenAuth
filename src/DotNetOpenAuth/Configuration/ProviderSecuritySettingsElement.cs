//-----------------------------------------------------------------------
// <copyright file="ProviderSecuritySettingsElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// Represents the .config file element that allows for setting the security policies of the Provider.
	/// </summary>
	internal class ProviderSecuritySettingsElement : ConfigurationElement {
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
		/// Initializes a new instance of the <see cref="ProviderSecuritySettingsElement"/> class.
		/// </summary>
		public ProviderSecuritySettingsElement() {
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
		/// Gets or sets a value indicating whether the Provider should take special care to protect OpenID 1.x relying parties
		/// against replay attacks.
		/// </summary>
		[ConfigurationProperty(ProtectDownlevelReplayAttacksConfigName, DefaultValue = false)]
		public bool ProtectDownlevelReplayAttacks {
			get { return (bool)this[ProtectDownlevelReplayAttacksConfigName]; }
			set { this[ProtectDownlevelReplayAttacksConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the configured lifetimes of the various association types.
		/// </summary>
		[ConfigurationProperty(AssociationsConfigName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(AssociationTypeCollection))]
		public AssociationTypeCollection AssociationLifetimes {
			get { return (AssociationTypeCollection)this[AssociationsConfigName] ?? new AssociationTypeCollection(); }
			set { this[AssociationsConfigName] = value; }
		}

		/// <summary>
		/// Initializes a programmatically manipulatable bag of these security settings with the settings from the config file.
		/// </summary>
		/// <returns>The newly created security settings object.</returns>
		public ProviderSecuritySettings CreateSecuritySettings() {
			ProviderSecuritySettings settings = new ProviderSecuritySettings();
			settings.MinimumHashBitLength = this.MinimumHashBitLength;
			settings.MaximumHashBitLength = this.MaximumHashBitLength;
			settings.ProtectDownlevelReplayAttacks = this.ProtectDownlevelReplayAttacks;
			foreach (AssociationTypeElement element in this.AssociationLifetimes) {
				settings.AssociationLifetimes.Add(element.AssociationType, element.MaximumLifetime);
			}
			return settings;
		}
	}
}
