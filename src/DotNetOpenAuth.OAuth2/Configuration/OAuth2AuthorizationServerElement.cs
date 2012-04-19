//-----------------------------------------------------------------------
// <copyright file="OAuth2AuthorizationServerElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// Represents the &lt;oauth2/authorizationServer&gt; element in the host's .config file.
	/// </summary>
	internal class OAuth2AuthorizationServerElement : ConfigurationElement {
		/// <summary>
		/// The name of the &lt;clientAuthenticationModules&gt; sub-element.
		/// </summary>
		private const string ClientAuthenticationModulesElementName = "clientAuthenticationModules";

		/// <summary>
		/// The built-in set of identifier discovery services.
		/// </summary>
		private static readonly TypeConfigurationCollection<IClientAuthenticationModule> defaultClientAuthenticationModules =
			new TypeConfigurationCollection<IClientAuthenticationModule>();

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2AuthorizationServerElement"/> class.
		/// </summary>
		internal OAuth2AuthorizationServerElement() {
		}

		/// <summary>
		/// Gets or sets the services to use for discovering service endpoints for identifiers.
		/// </summary>
		/// <remarks>
		/// If no discovery services are defined in the (web) application's .config file,
		/// the default set of discovery services built into the library are used.
		/// </remarks>
		[ConfigurationProperty(ClientAuthenticationModulesElementName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(TypeConfigurationCollection<IClientAuthenticationModule>))]
		internal TypeConfigurationCollection<IClientAuthenticationModule> ClientAuthenticationModules {
			get {
				var configResult = (TypeConfigurationCollection<IClientAuthenticationModule>)this[ClientAuthenticationModulesElementName];
				return configResult != null && configResult.Count > 0 ? configResult : defaultClientAuthenticationModules;
			}

			set {
				this[ClientAuthenticationModulesElementName] = value;
			}
		}
	}
}
