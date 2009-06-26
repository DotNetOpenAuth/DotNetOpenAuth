//-----------------------------------------------------------------------
// <copyright file="OAuthServiceProviderElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;

	/// <summary>
	/// Represents the &lt;oauth/serviceProvider&gt; element in the host's .config file.
	/// </summary>
	internal class OAuthServiceProviderElement : ConfigurationElement {
		/// <summary>
		/// Gets the name of the security sub-element.
		/// </summary>
		private const string SecuritySettingsConfigName = "security";

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthServiceProviderElement"/> class.
		/// </summary>
		internal OAuthServiceProviderElement() {
		}

		/// <summary>
		/// Gets or sets the security settings.
		/// </summary>
		[ConfigurationProperty(SecuritySettingsConfigName)]
		public OAuthServiceProviderSecuritySettingsElement SecuritySettings {
			get { return (OAuthServiceProviderSecuritySettingsElement)this[SecuritySettingsConfigName] ?? new OAuthServiceProviderSecuritySettingsElement(); }
			set { this[SecuritySettingsConfigName] = value; }
		}
	}
}
