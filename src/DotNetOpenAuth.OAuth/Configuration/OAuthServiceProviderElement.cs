//-----------------------------------------------------------------------
// <copyright file="OAuthServiceProviderElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// Represents the &lt;oauth/serviceProvider&gt; element in the host's .config file.
	/// </summary>
	internal class OAuthServiceProviderElement : ConfigurationElement {
		/// <summary>
		/// The name of the custom store sub-element.
		/// </summary>
		private const string StoreConfigName = "store";

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
		/// Gets or sets the type to use for storing application state.
		/// </summary>
		[ConfigurationProperty(StoreConfigName)]
		public TypeConfigurationElement<INonceStore> ApplicationStore {
			get { return (TypeConfigurationElement<INonceStore>)this[StoreConfigName] ?? new TypeConfigurationElement<INonceStore>(); }
			set { this[StoreConfigName] = value; }
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
