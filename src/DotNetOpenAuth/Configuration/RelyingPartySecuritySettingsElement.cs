//-----------------------------------------------------------------------
// <copyright file="RelyingPartySecuritySettingsElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
	internal class RelyingPartySecuritySettingsElement : ConfigurationElement {
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
		/// Gets the name of the @privateSecretMaximumAge attribute.
		/// </summary>
		private const string PrivateSecretMaximumAgeConfigName = "privateSecretMaximumAge";

		/// <summary>
		/// Initializes a new instance of the <see cref="RelyingPartySecuritySettingsElement"/> class.
		/// </summary>
		public RelyingPartySecuritySettingsElement() {
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
		/// Gets or sets the maximum allowable age of the secret a Relying Party
		/// uses to its return_to URLs and nonces with 1.0 Providers.
		/// </summary>
		/// <value>The default value is 7 days.</value>
		[ConfigurationProperty(PrivateSecretMaximumAgeConfigName, DefaultValue = "07:00:00")]
		public TimeSpan PrivateSecretMaximumAge {
			get { return (TimeSpan)this[PrivateSecretMaximumAgeConfigName]; }
			set { this[PrivateSecretMaximumAgeConfigName] = value; }
		}

		/// <summary>
		/// Initializes a programmatically manipulatable bag of these security settings with the settings from the config file.
		/// </summary>
		/// <returns>The newly created security settings object.</returns>
		public RelyingPartySecuritySettings CreateSecuritySettings() {
			RelyingPartySecuritySettings settings = new RelyingPartySecuritySettings();
			settings.RequireSsl = this.RequireSsl;
			settings.MinimumRequiredOpenIdVersion = this.MinimumRequiredOpenIdVersion;
			settings.MinimumHashBitLength = this.MinimumHashBitLength;
			settings.MaximumHashBitLength = this.MaximumHashBitLength;
			settings.PrivateSecretMaximumAge = this.PrivateSecretMaximumAge;
			return settings;
		}
	}
}
