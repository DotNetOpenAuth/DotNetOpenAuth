//-----------------------------------------------------------------------
// <copyright file="OpenIdElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Represents the &lt;openid&gt; element in the host's .config file.
	/// </summary>
	[ContractVerification(true)]
	internal class OpenIdElement : ConfigurationElement {
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
		/// Gets the name of the @maxAuthenticationTime attribute.
		/// </summary>
		private const string MaxAuthenticationTimePropertyName = "maxAuthenticationTime";

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdElement"/> class.
		/// </summary>
		internal OpenIdElement() {
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
				Contract.Ensures(Contract.Result<TimeSpan>() > TimeSpan.Zero);
				return (TimeSpan)this[MaxAuthenticationTimePropertyName];
			}

			set {
				Contract.Requires(value > TimeSpan.Zero);
				this[MaxAuthenticationTimePropertyName] = value;
			}
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
		/// Gets or sets the registered OpenID extensions.
		/// </summary>
		[ConfigurationProperty(ExtensionFactoriesElementName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(TypeConfigurationCollection<IOpenIdMessageExtension>))]
		internal TypeConfigurationCollection<IOpenIdExtensionFactory> ExtensionFactories {
			get { return (TypeConfigurationCollection<IOpenIdExtensionFactory>)this[ExtensionFactoriesElementName] ?? new TypeConfigurationCollection<IOpenIdExtensionFactory>(); }
			set { this[ExtensionFactoriesElementName] = value; }
		}
	}
}
