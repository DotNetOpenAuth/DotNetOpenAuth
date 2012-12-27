//-----------------------------------------------------------------------
// <copyright file="OAuth2AuthorizationServerSection.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// Represents the &lt;oauth2/authorizationServer&gt; section in the host's .config file.
	/// </summary>
	internal class OAuth2AuthorizationServerSection : ConfigurationSection {
		/// <summary>
		/// The name of the oauth2/authorizationServer section.
		/// </summary>
		private const string SectionName = OAuth2SectionGroup.SectionName + "/authorizationServer";

		/// <summary>
		/// The name of the &lt;clientAuthenticationModules&gt; sub-element.
		/// </summary>
		private const string ClientAuthenticationModulesElementName = "clientAuthenticationModules";

		/// <summary>
		/// The built-in set of client authentication modules.
		/// </summary>
		private static readonly TypeConfigurationCollection<ClientAuthenticationModule> defaultClientAuthenticationModules =
			new TypeConfigurationCollection<ClientAuthenticationModule>(new Type[] { typeof(ClientCredentialHttpBasicReader), typeof(ClientCredentialMessagePartReader) });

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2AuthorizationServerSection"/> class.
		/// </summary>
		internal OAuth2AuthorizationServerSection() {
		}

		/// <summary>
		/// Gets the configuration section from the .config file.
		/// </summary>
		internal static OAuth2AuthorizationServerSection Configuration {
			get {
				return (OAuth2AuthorizationServerSection)ConfigurationManager.GetSection(SectionName) ?? new OAuth2AuthorizationServerSection();
			}
		}

		/// <summary>
		/// Gets or sets the services to use for discovering service endpoints for identifiers.
		/// </summary>
		/// <remarks>
		/// If no discovery services are defined in the (web) application's .config file,
		/// the default set of discovery services built into the library are used.
		/// </remarks>
		[ConfigurationProperty(ClientAuthenticationModulesElementName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(TypeConfigurationCollection<ClientAuthenticationModule>))]
		internal TypeConfigurationCollection<ClientAuthenticationModule> ClientAuthenticationModules {
			get {
				var configResult = (TypeConfigurationCollection<ClientAuthenticationModule>)this[ClientAuthenticationModulesElementName];
				return configResult != null && configResult.Count > 0 ? configResult : defaultClientAuthenticationModules;
			}

			set {
				this[ClientAuthenticationModulesElementName] = value;
			}
		}
	}
}
