//-----------------------------------------------------------------------
// <copyright file="OAuthServiceProviderSecuritySettingsElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth;

	/// <summary>
	/// Security settings that are applicable to service providers.
	/// </summary>
	internal class OAuthServiceProviderSecuritySettingsElement : ConfigurationElement {
		/// <summary>
		/// Gets the name of the @minimumRequiredOAuthVersion attribute.
		/// </summary>
		private const string MinimumRequiredOAuthVersionConfigName = "minimumRequiredOAuthVersion";

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthServiceProviderSecuritySettingsElement"/> class.
		/// </summary>
		internal OAuthServiceProviderSecuritySettingsElement() {
		}

		/// <summary>
		/// Gets or sets the minimum OAuth version a Consumer is required to support in order for this library to interoperate with it.
		/// </summary>
		/// <remarks>
		/// Although the earliest versions of OAuth are supported, for security reasons it may be desirable to require the
		/// remote party to support a later version of OAuth.
		/// </remarks>
		[ConfigurationProperty(MinimumRequiredOAuthVersionConfigName, DefaultValue = "V10")]
		public ProtocolVersion MinimumRequiredOAuthVersion {
			get { return (ProtocolVersion)this[MinimumRequiredOAuthVersionConfigName]; }
			set { this[MinimumRequiredOAuthVersionConfigName] = value; }
		}

		/// <summary>
		/// Initializes a programmatically manipulatable bag of these security settings with the settings from the config file.
		/// </summary>
		/// <returns>The newly created security settings object.</returns>
		internal ServiceProviderSecuritySettings CreateSecuritySettings() {
			return new ServiceProviderSecuritySettings {
				MinimumRequiredOAuthVersion = this.MinimumRequiredOAuthVersion,
			};
		}
	}
}
