//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// The section in the .config file that allows customization of OpenID Relying Party behaviors.
	/// </summary>
	[ContractVerification(true)]
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
		/// Gets the name of the &lt;behaviors&gt; sub-element.
		/// </summary>
		private const string BehaviorsElementName = "behaviors";

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingPartyElement"/> class.
		/// </summary>
		public OpenIdRelyingPartyElement() {
		}

		/// <summary>
		/// Gets or sets the security settings.
		/// </summary>
		[ConfigurationProperty(SecuritySettingsConfigName)]
		public OpenIdRelyingPartySecuritySettingsElement SecuritySettings {
			get { return (OpenIdRelyingPartySecuritySettingsElement)this[SecuritySettingsConfigName] ?? new OpenIdRelyingPartySecuritySettingsElement(); }
			set { this[SecuritySettingsConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the special behaviors to apply.
		/// </summary>
		[ConfigurationProperty(BehaviorsElementName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(TypeConfigurationCollection<IRelyingPartyBehavior>))]
		public TypeConfigurationCollection<IRelyingPartyBehavior> Behaviors {
			get { return (TypeConfigurationCollection<IRelyingPartyBehavior>)this[BehaviorsElementName] ?? new TypeConfigurationCollection<IRelyingPartyBehavior>(); }
			set { this[BehaviorsElementName] = value; }
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
