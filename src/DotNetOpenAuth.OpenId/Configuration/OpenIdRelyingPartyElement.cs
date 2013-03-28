//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;

	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
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
		/// The name of the &lt;relyingParty&gt; sub-element.
		/// </summary>
		private const string RelyingPartyElementName = "relyingParty";

		/// <summary>
		/// The name of the attribute that specifies whether dnoa.userSuppliedIdentifier is tacked onto the openid.return_to URL.
		/// </summary>
		private const string PreserveUserSuppliedIdentifierConfigName = "preserveUserSuppliedIdentifier";

		/// <summary>
		/// Gets the name of the security sub-element.
		/// </summary>
		private const string SecuritySettingsConfigName = "security";

		/// <summary>
		/// The name of the &lt;behaviors&gt; sub-element.
		/// </summary>
		private const string BehaviorsElementName = "behaviors";

		/// <summary>
		/// The name of the &lt;discoveryServices&gt; sub-element.
		/// </summary>
		private const string DiscoveryServicesElementName = "discoveryServices";

		/// <summary>
		/// The name of the &lt;hostMetaDiscovery&gt; sub-element.
		/// </summary>
		private const string HostMetaDiscoveryElementName = "hostMetaDiscovery";

		/// <summary>
		/// The built-in set of identifier discovery services.
		/// </summary>
		private static readonly TypeConfigurationCollection<IIdentifierDiscoveryService> defaultDiscoveryServices =
			new TypeConfigurationCollection<IIdentifierDiscoveryService>(new Type[] { typeof(UriDiscoveryService), typeof(XriDiscoveryProxyService) });

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingPartyElement"/> class.
		/// </summary>
		public OpenIdRelyingPartyElement() {
		}

		/// <summary>
		/// Gets or sets a value indicating whether "dnoa.userSuppliedIdentifier" is tacked onto the openid.return_to URL in order to preserve what the user typed into the OpenID box.
		/// </summary>
		/// <value>
		/// 	The default value is <c>true</c>.
		/// </value>
		[ConfigurationProperty(PreserveUserSuppliedIdentifierConfigName, DefaultValue = true)]
		public bool PreserveUserSuppliedIdentifier {
			get { return (bool)this[PreserveUserSuppliedIdentifierConfigName]; }
			set { this[PreserveUserSuppliedIdentifierConfigName] = value; }
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
		public TypeConfigurationElement<ICryptoKeyAndNonceStore> ApplicationStore {
			get { return (TypeConfigurationElement<ICryptoKeyAndNonceStore>)this[StoreConfigName] ?? new TypeConfigurationElement<ICryptoKeyAndNonceStore>(); }
			set { this[StoreConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the host meta discovery configuration element.
		/// </summary>
		[ConfigurationProperty(HostMetaDiscoveryElementName)]
		internal HostMetaDiscoveryElement HostMetaDiscovery {
			get { return (HostMetaDiscoveryElement)this[HostMetaDiscoveryElementName] ?? new HostMetaDiscoveryElement(); }
			set { this[HostMetaDiscoveryElementName] = value; }
		}

		/// <summary>
		/// Gets or sets the services to use for discovering service endpoints for identifiers.
		/// </summary>
		/// <remarks>
		/// If no discovery services are defined in the (web) application's .config file,
		/// the default set of discovery services built into the library are used.
		/// </remarks>
		[ConfigurationProperty(DiscoveryServicesElementName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(TypeConfigurationCollection<IIdentifierDiscoveryService>))]
		internal TypeConfigurationCollection<IIdentifierDiscoveryService> DiscoveryServices {
			get {
				var configResult = (TypeConfigurationCollection<IIdentifierDiscoveryService>)this[DiscoveryServicesElementName];
				return configResult != null && configResult.Count > 0 ? configResult : defaultDiscoveryServices;
			}

			set {
				this[DiscoveryServicesElementName] = value;
			}
		}
	}
}
