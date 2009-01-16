//-----------------------------------------------------------------------
// <copyright file="RelyingPartySection.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// The section in the .config file that allows customization of OpenID Relying Party behaviors.
	/// </summary>
	internal class RelyingPartySection : ConfigurationSection {
		/// <summary>
		/// The path to the section in a .config file where these settings can be given.
		/// </summary>
		private const string SectionName = "dotNetOpenAuth/openid/relyingParty";

		/// <summary>
		/// The name of the custom store sub-element.
		/// </summary>
		private const string StoreConfigName = "store";

		/// <summary>
		/// Gets the name of the security sub-element.
		/// </summary>
		private const string SecuritySettingsConfigName = "security";

		/// <summary>
		/// Initializes a new instance of the <see cref="RelyingPartySection"/> class.
		/// </summary>
		public RelyingPartySection() {
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

		/// <summary>
		/// Gets the configuration element from the .config file.
		/// </summary>
		internal static RelyingPartySection Configuration {
			get { return (RelyingPartySection)ConfigurationManager.GetSection(SectionName) ?? new RelyingPartySection(); }
		}
	}
}
