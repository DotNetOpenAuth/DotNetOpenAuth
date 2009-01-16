//-----------------------------------------------------------------------
// <copyright file="OpenIdProviderElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// The section in the .config file that allows customization of OpenID Provider behaviors.
	/// </summary>
	internal class OpenIdProviderElement : ConfigurationElement {
		/// <summary>
		/// The name of the security sub-element.
		/// </summary>
		private const string SecuritySettingsConfigName = "security";

		/// <summary>
		/// The name of the custom store sub-element.
		/// </summary>
		private const string StoreConfigName = "store";

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProviderElement"/> class.
		/// </summary>
		public OpenIdProviderElement() {
		}

		/// <summary>
		/// Gets or sets the security settings.
		/// </summary>
		[ConfigurationProperty(SecuritySettingsConfigName)]
		public ProviderSecuritySettingsElement SecuritySettings {
			get { return (ProviderSecuritySettingsElement)this[SecuritySettingsConfigName] ?? new ProviderSecuritySettingsElement(); }
			set { this[SecuritySettingsConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the type to use for storing application state.
		/// </summary>
		[ConfigurationProperty(StoreConfigName)]
		public TypeConfigurationElement<IProviderApplicationStore> ApplicationStore {
			get { return (TypeConfigurationElement<IProviderApplicationStore>)this[StoreConfigName] ?? new TypeConfigurationElement<IProviderApplicationStore>(); }
			set { this[StoreConfigName] = value; }
		}
	}
}
