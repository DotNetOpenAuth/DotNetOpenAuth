//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// The section in the .config file that allows customization of OpenID Relying Party behaviors.
	/// </summary>
	internal class OpenIdRelyingPartyElement : ConfigurationElement {
		/// <summary>
		/// The name of the custom store sub-element.
		/// </summary>
		private const string StoreConfigName = "store";

		/// <summary>
		/// Gets the name of the security sub-element.
		/// </summary>
		private const string SecuritySettingsConfigName = "security";

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingPartyElement"/> class.
		/// </summary>
		public OpenIdRelyingPartyElement() {
		}

		/// <summary>
		/// Gets or sets the security settings.
		/// </summary>
		[ConfigurationProperty(SecuritySettingsConfigName)]
		public RelyingPartySecuritySettingsElement SecuritySettings {
			get { return (RelyingPartySecuritySettingsElement)this[SecuritySettingsConfigName] ?? new RelyingPartySecuritySettingsElement(); }
			set { this[SecuritySettingsConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the type to use for storing application state.
		/// </summary>
		[ConfigurationProperty(StoreConfigName)]
		public TypeConfigurationElement<IRelyingPartyApplicationStore> ApplicationStore {
			get { return (TypeConfigurationElement<IRelyingPartyApplicationStore>)this[StoreConfigName] ?? new TypeConfigurationElement<IRelyingPartyApplicationStore>(); }
			set { this[StoreConfigName] = value; }
		}
	}
}
