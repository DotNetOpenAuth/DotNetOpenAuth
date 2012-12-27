//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartySecuritySettingsElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// Represents the .config file element that allows for setting the security policies of the Relying Party.
	/// </summary>
	internal class OpenIdRelyingPartySecuritySettingsElement : ConfigurationElement {
		/// <summary>
		/// Gets the name of the @minimumRequiredOpenIdVersion attribute.
		/// </summary>
		private const string MinimumRequiredOpenIdVersionConfigName = "minimumRequiredOpenIdVersion";

		/// <summary>
		/// Gets the name of the @minimumHashBitLength attribute.
		/// </summary>
		private const string MinimumHashBitLengthConfigName = "minimumHashBitLength";

		/// <summary>
		/// Gets the name of the @maximumHashBitLength attribute.
		/// </summary>
		private const string MaximumHashBitLengthConfigName = "maximumHashBitLength";

		/// <summary>
		/// Gets the name of the @requireSsl attribute.
		/// </summary>
		private const string RequireSslConfigName = "requireSsl";

		/// <summary>
		/// Gets the name of the @requireDirectedIdentity attribute.
		/// </summary>
		private const string RequireDirectedIdentityConfigName = "requireDirectedIdentity";

		/// <summary>
		/// Gets the name of the @requireAssociation attribute.
		/// </summary>
		private const string RequireAssociationConfigName = "requireAssociation";

		/// <summary>
		/// Gets the name of the @rejectUnsolicitedAssertions attribute.
		/// </summary>
		private const string RejectUnsolicitedAssertionsConfigName = "rejectUnsolicitedAssertions";

		/// <summary>
		/// Gets the name of the @rejectDelegatedIdentifiers attribute.
		/// </summary>
		private const string RejectDelegatingIdentifiersConfigName = "rejectDelegatingIdentifiers";

		/// <summary>
		/// Gets the name of the @ignoreUnsignedExtensions attribute.
		/// </summary>
		private const string IgnoreUnsignedExtensionsConfigName = "ignoreUnsignedExtensions";

		/// <summary>
		/// Gets the name of the @allowDualPurposeIdentifiers attribute.
		/// </summary>
		private const string AllowDualPurposeIdentifiersConfigName = "allowDualPurposeIdentifiers";

		/// <summary>
		/// Gets the name of the @allowApproximateIdentifierDiscovery attribute.
		/// </summary>
		private const string AllowApproximateIdentifierDiscoveryConfigName = "allowApproximateIdentifierDiscovery";

		/// <summary>
		/// Gets the name of the @protectDownlevelReplayAttacks attribute.
		/// </summary>
		private const string ProtectDownlevelReplayAttacksConfigName = "protectDownlevelReplayAttacks";

		/// <summary>
		/// The name of the &lt;trustedProviders&gt; sub-element.
		/// </summary>
		private const string TrustedProvidersElementName = "trustedProviders";

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingPartySecuritySettingsElement"/> class.
		/// </summary>
		public OpenIdRelyingPartySecuritySettingsElement() {
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
		/// Gets or sets a value indicating whether only OP Identifiers will be discoverable 
		/// when creating authentication requests.
		/// </summary>
		[ConfigurationProperty(RequireDirectedIdentityConfigName, DefaultValue = false)]
		public bool RequireDirectedIdentity {
			get { return (bool)this[RequireDirectedIdentityConfigName]; }
			set { this[RequireDirectedIdentityConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether authentication requests
		/// will only be created where an association with the Provider can be established.
		/// </summary>
		[ConfigurationProperty(RequireAssociationConfigName, DefaultValue = false)]
		public bool RequireAssociation {
			get { return (bool)this[RequireAssociationConfigName]; }
			set { this[RequireAssociationConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the minimum OpenID version a Provider is required to support in order for this library to interoperate with it.
		/// </summary>
		/// <remarks>
		/// Although the earliest versions of OpenID are supported, for security reasons it may be desirable to require the
		/// remote party to support a later version of OpenID.
		/// </remarks>
		[ConfigurationProperty(MinimumRequiredOpenIdVersionConfigName, DefaultValue = "V10")]
		public ProtocolVersion MinimumRequiredOpenIdVersion {
			get { return (ProtocolVersion)this[MinimumRequiredOpenIdVersionConfigName]; }
			set { this[MinimumRequiredOpenIdVersionConfigName] = value; }
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
		/// Gets or sets a value indicating whether all unsolicited assertions should be ignored.
		/// </summary>
		/// <value>The default value is <c>false</c>.</value>
		[ConfigurationProperty(RejectUnsolicitedAssertionsConfigName, DefaultValue = false)]
		public bool RejectUnsolicitedAssertions {
			get { return (bool)this[RejectUnsolicitedAssertionsConfigName]; }
			set { this[RejectUnsolicitedAssertionsConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether delegating identifiers are refused for authentication.
		/// </summary>
		/// <value>The default value is <c>false</c>.</value>
		/// <remarks>
		/// When set to <c>true</c>, login attempts that start at the RP or arrive via unsolicited
		/// assertions will be rejected if discovery on the identifier shows that OpenID delegation
		/// is used for the identifier.  This is useful for an RP that should only accept identifiers
		/// directly issued by the Provider that is sending the assertion.
		/// </remarks>
		[ConfigurationProperty(RejectDelegatingIdentifiersConfigName, DefaultValue = false)]
		public bool RejectDelegatingIdentifiers {
			get { return (bool)this[RejectDelegatingIdentifiersConfigName]; }
			set { this[RejectDelegatingIdentifiersConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether unsigned extensions in authentication responses should be ignored.
		/// </summary>
		/// <value>The default value is <c>false</c>.</value>
		/// <remarks>
		/// When set to true, the <see cref="IAuthenticationResponse.GetUntrustedExtension"/> methods
		/// will not return any extension that was not signed by the Provider.
		/// </remarks>
		[ConfigurationProperty(IgnoreUnsignedExtensionsConfigName, DefaultValue = false)]
		public bool IgnoreUnsignedExtensions {
			get { return (bool)this[IgnoreUnsignedExtensionsConfigName]; }
			set { this[IgnoreUnsignedExtensionsConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether identifiers that are both OP Identifiers and Claimed Identifiers
		/// should ever be recognized as claimed identifiers.
		/// </summary>
		/// <value>
		/// 	The default value is <c>false</c>, per the OpenID 2.0 spec.
		/// </value>
		[ConfigurationProperty(AllowDualPurposeIdentifiersConfigName, DefaultValue = false)]
		public bool AllowDualPurposeIdentifiers {
			get { return (bool)this[AllowDualPurposeIdentifiersConfigName]; }
			set { this[AllowDualPurposeIdentifiersConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether certain Claimed Identifiers that exploit
		/// features that .NET does not have the ability to send exact HTTP requests for will
		/// still be allowed by using an approximate HTTP request.
		/// </summary>
		/// <value>
		/// 	The default value is <c>true</c>.
		/// </value>
		[ConfigurationProperty(AllowApproximateIdentifierDiscoveryConfigName, DefaultValue = true)]
		public bool AllowApproximateIdentifierDiscovery {
			get { return (bool)this[AllowApproximateIdentifierDiscoveryConfigName]; }
			set { this[AllowApproximateIdentifierDiscoveryConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the Relying Party should take special care 
		/// to protect users against replay attacks when interoperating with OpenID 1.1 Providers.
		/// </summary>
		[ConfigurationProperty(ProtectDownlevelReplayAttacksConfigName, DefaultValue = RelyingPartySecuritySettings.ProtectDownlevelReplayAttacksDefault)]
		public bool ProtectDownlevelReplayAttacks {
			get { return (bool)this[ProtectDownlevelReplayAttacksConfigName]; }
			set { this[ProtectDownlevelReplayAttacksConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the set of trusted OpenID Provider Endpoints.
		/// </summary>
		[ConfigurationProperty(TrustedProvidersElementName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(TrustedProviderConfigurationCollection))]
		public TrustedProviderConfigurationCollection TrustedProviders {
			get { return (TrustedProviderConfigurationCollection)this[TrustedProvidersElementName] ?? new TrustedProviderConfigurationCollection(); }
			set { this[TrustedProvidersElementName] = value; }
		}

		/// <summary>
		/// Initializes a programmatically manipulatable bag of these security settings with the settings from the config file.
		/// </summary>
		/// <returns>The newly created security settings object.</returns>
		public RelyingPartySecuritySettings CreateSecuritySettings() {
			RelyingPartySecuritySettings settings = new RelyingPartySecuritySettings();
			settings.RequireSsl = this.RequireSsl;
			settings.RequireDirectedIdentity = this.RequireDirectedIdentity;
			settings.RequireAssociation = this.RequireAssociation;
			settings.MinimumRequiredOpenIdVersion = this.MinimumRequiredOpenIdVersion;
			settings.MinimumHashBitLength = this.MinimumHashBitLength;
			settings.MaximumHashBitLength = this.MaximumHashBitLength;
			settings.PrivateSecretMaximumAge = DotNetOpenAuthSection.Messaging.PrivateSecretMaximumAge;
			settings.RejectUnsolicitedAssertions = this.RejectUnsolicitedAssertions;
			settings.RejectDelegatingIdentifiers = this.RejectDelegatingIdentifiers;
			settings.IgnoreUnsignedExtensions = this.IgnoreUnsignedExtensions;
			settings.AllowDualPurposeIdentifiers = this.AllowDualPurposeIdentifiers;
			settings.AllowApproximateIdentifierDiscovery = this.AllowApproximateIdentifierDiscovery;
			settings.ProtectDownlevelReplayAttacks = this.ProtectDownlevelReplayAttacks;

			settings.RejectAssertionsFromUntrustedProviders = this.TrustedProviders.RejectAssertionsFromUntrustedProviders;
			foreach (TrustedProviderEndpointConfigurationElement opEndpoint in this.TrustedProviders) {
				settings.TrustedProviderEndpoints.Add(opEndpoint.ProviderEndpoint);
			}

			return settings;
		}
	}
}
