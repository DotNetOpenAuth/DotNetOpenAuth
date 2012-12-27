//-----------------------------------------------------------------------
// <copyright file="OpenIdElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// Represents the &lt;openid&gt; element in the host's .config file.
	/// </summary>
	internal class OpenIdElement : ConfigurationSection {
		/// <summary>
		/// The name of the section under which this library's settings must be found.
		/// </summary>
		private const string SectionName = DotNetOpenAuthSection.SectionName + "/openid";

		/// <summary>
		/// The name of the &lt;relyingParty&gt; sub-element.
		/// </summary>
		private const string RelyingPartyElementName = "relyingParty";

		/// <summary>
		/// The name of the &lt;provider&gt; sub-element.
		/// </summary>
		private const string ProviderElementName = "provider";

		/// <summary>
		/// The name of the &lt;extensions&gt; sub-element.
		/// </summary>
		private const string ExtensionFactoriesElementName = "extensionFactories";

		/// <summary>
		/// The name of the &lt;xriResolver&gt; sub-element.
		/// </summary>
		private const string XriResolverElementName = "xriResolver";

		/// <summary>
		/// The name of the @maxAuthenticationTime attribute.
		/// </summary>
		private const string MaxAuthenticationTimePropertyName = "maxAuthenticationTime";

		/// <summary>
		/// The name of the @cacheDiscovery attribute.
		/// </summary>
		private const string CacheDiscoveryPropertyName = "cacheDiscovery";

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdElement"/> class.
		/// </summary>
		internal OpenIdElement() {
		}

		/// <summary>
		/// Gets the configuration section from the .config file.
		/// </summary>
		public static OpenIdElement Configuration {
			get {
				return (OpenIdElement)ConfigurationManager.GetSection(SectionName) ?? new OpenIdElement();
			}
		}

		/// <summary>
		/// Gets or sets the maximum time a user can take to complete authentication.
		/// </summary>
		/// <remarks>
		/// This time limit allows the library to decide how long to cache certain values
		/// necessary to complete authentication.  The lower the time, the less demand on
		/// the server.  But too short a time can frustrate the user.
		/// </remarks>
		[ConfigurationProperty(MaxAuthenticationTimePropertyName, DefaultValue = "0:05")] // 5 minutes
		[PositiveTimeSpanValidator]
		internal TimeSpan MaxAuthenticationTime {
			get {
				TimeSpan result = (TimeSpan)this[MaxAuthenticationTimePropertyName];
				Assumes.True(result > TimeSpan.Zero); // our PositiveTimeSpanValidator should take care of this
				return result;
			}

			set {
				Requires.Range(value > TimeSpan.Zero, "value");
				this[MaxAuthenticationTimePropertyName] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the results of Identifier discovery
		/// should be cached.
		/// </summary>
		/// <value>
		/// Use <c>true</c> to allow identifier discovery to immediately return cached results when available;
		/// otherwise, use <c>false</c>.to force fresh results every time at the cost of slightly slower logins.
		/// The default value is <c>true</c>.
		/// </value>
		/// <remarks>
		/// When enabled, caching is done according to HTTP standards.
		/// </remarks>
		[ConfigurationProperty(CacheDiscoveryPropertyName, DefaultValue = true)]
		internal bool CacheDiscovery {
			get { return (bool)this[CacheDiscoveryPropertyName]; }
			set { this[CacheDiscoveryPropertyName] = value; }
		}

		/// <summary>
		/// Gets or sets the configuration specific for Relying Parties.
		/// </summary>
		[ConfigurationProperty(RelyingPartyElementName)]
		internal OpenIdRelyingPartyElement RelyingParty {
			get { return (OpenIdRelyingPartyElement)this[RelyingPartyElementName] ?? new OpenIdRelyingPartyElement(); }
			set { this[RelyingPartyElementName] = value; }
		}

		/// <summary>
		/// Gets or sets the configuration specific for Providers.
		/// </summary>
		[ConfigurationProperty(ProviderElementName)]
		internal OpenIdProviderElement Provider {
			get { return (OpenIdProviderElement)this[ProviderElementName] ?? new OpenIdProviderElement(); }
			set { this[ProviderElementName] = value; }
		}

		/// <summary>
		/// Gets or sets the registered OpenID extension factories.
		/// </summary>
		[ConfigurationProperty(ExtensionFactoriesElementName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(TypeConfigurationCollection<IOpenIdExtensionFactory>))]
		internal TypeConfigurationCollection<IOpenIdExtensionFactory> ExtensionFactories {
			get { return (TypeConfigurationCollection<IOpenIdExtensionFactory>)this[ExtensionFactoriesElementName] ?? new TypeConfigurationCollection<IOpenIdExtensionFactory>(); }
			set { this[ExtensionFactoriesElementName] = value; }
		}

		/// <summary>
		/// Gets or sets the configuration for the XRI resolver.
		/// </summary>
		[ConfigurationProperty(XriResolverElementName)]
		internal XriResolverElement XriResolver {
			get { return (XriResolverElement)this[XriResolverElementName] ?? new XriResolverElement(); }
			set { this[XriResolverElementName] = value; }
		}
	}
}
