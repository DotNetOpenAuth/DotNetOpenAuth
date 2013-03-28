//-----------------------------------------------------------------------
// <copyright file="OpenIdProviderElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;

	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// The section in the .config file that allows customization of OpenID Provider behaviors.
	/// </summary>
	internal class OpenIdProviderElement : ConfigurationElement {
		/// <summary>
		/// The name of the &lt;provider&gt; sub-element.
		/// </summary>
		private const string ProviderElementName = "provider";

		/// <summary>
		/// The name of the security sub-element.
		/// </summary>
		private const string SecuritySettingsConfigName = "security";

		/// <summary>
		/// Gets the name of the &lt;behaviors&gt; sub-element.
		/// </summary>
		private const string BehaviorsElementName = "behaviors";

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
		public OpenIdProviderSecuritySettingsElement SecuritySettings {
			get { return (OpenIdProviderSecuritySettingsElement)this[SecuritySettingsConfigName] ?? new OpenIdProviderSecuritySettingsElement(); }
			set { this[SecuritySettingsConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the special behaviors to apply.
		/// </summary>
		[ConfigurationProperty(BehaviorsElementName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(TypeConfigurationCollection<IProviderBehavior>))]
		public TypeConfigurationCollection<IProviderBehavior> Behaviors {
			get { return (TypeConfigurationCollection<IProviderBehavior>)this[BehaviorsElementName] ?? new TypeConfigurationCollection<IProviderBehavior>(); }
			set { this[BehaviorsElementName] = value; }
		}

		/// <summary>
		/// Gets or sets the type to use for storing application state.
		/// </summary>
		[ConfigurationProperty(StoreConfigName)]
		public TypeConfigurationElement<ICryptoKeyAndNonceStore> ApplicationStore {
			get { return (TypeConfigurationElement<ICryptoKeyAndNonceStore>)this[StoreConfigName] ?? new TypeConfigurationElement<ICryptoKeyAndNonceStore>(); }
			set { this[StoreConfigName] = value; }
		}
	}
}
