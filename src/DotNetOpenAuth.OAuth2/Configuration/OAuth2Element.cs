//-----------------------------------------------------------------------
// <copyright file="OAuth2Element.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// Represents the &lt;oauth&gt; element in the host's .config file.
	/// </summary>
	internal class OAuth2Element : ConfigurationSection {
		/// <summary>
		/// The name of the oauth section.
		/// </summary>
		private const string SectionName = DotNetOpenAuthSection.SectionName + "/oauth2";

		/// <summary>
		/// The name of the &lt;client&gt; sub-element.
		/// </summary>
		private const string ClientElementName = "client";

		/// <summary>
		/// The name of the &lt;authorizationServer&gt; sub-element.
		/// </summary>
		private const string AuthorizationServerElementName = "authorizationServer";

		/// <summary>
		/// The name of the &lt;resourceServer&gt; sub-element.
		/// </summary>
		private const string ResourceServerElementName = "resourceServer";

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2Element"/> class.
		/// </summary>
		internal OAuth2Element() {
		}

		/// <summary>
		/// Gets the configuration section from the .config file.
		/// </summary>
		public static OAuth2Element Configuration {
			get {
				Contract.Ensures(Contract.Result<OAuth2Element>() != null);
				return (OAuth2Element)ConfigurationManager.GetSection(SectionName) ?? new OAuth2Element();
			}
		}

		/// <summary>
		/// Gets or sets the configuration specific for Clients.
		/// </summary>
		[ConfigurationProperty(ClientElementName)]
		internal OAuth2ClientElement Client {
			get { return (OAuth2ClientElement)this[ClientElementName] ?? new OAuth2ClientElement(); }
			set { this[ClientElementName] = value; }
		}

		/// <summary>
		/// Gets or sets the configuration specific for Authorization Servers.
		/// </summary>
		[ConfigurationProperty(AuthorizationServerElementName)]
		internal OAuth2AuthorizationServerElement AuthorizationServer {
			get { return (OAuth2AuthorizationServerElement)this[AuthorizationServerElementName] ?? new OAuth2AuthorizationServerElement(); }
			set { this[AuthorizationServerElementName] = value; }
		}

		/// <summary>
		/// Gets or sets the configuration specific for Resource Servers.
		/// </summary>
		[ConfigurationProperty(ResourceServerElementName)]
		internal OAuth2ResourceServerElement ResourceServer {
			get { return (OAuth2ResourceServerElement)this[ResourceServerElementName] ?? new OAuth2ResourceServerElement(); }
			set { this[ResourceServerElementName] = value; }
		}
	}
}
