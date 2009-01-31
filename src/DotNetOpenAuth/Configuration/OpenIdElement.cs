//-----------------------------------------------------------------------
// <copyright file="OpenIdElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;

	/// <summary>
	/// Represents the &lt;openid&gt; element in the host's .config file.
	/// </summary>
	internal class OpenIdElement : ConfigurationElement {
		/// <summary>
		/// Gets the name of the &lt;relyingParty&gt; sub-element.
		/// </summary>
		private const string RelyingPartyElementName = "relyingParty";

		/// <summary>
		/// Gets the name of the &lt;provider&gt; sub-element.
		/// </summary>
		private const string ProviderElementName = "provider";

		/// <summary>
		/// Gets the name of the @maxAuthenticationTime attribute.
		/// </summary>
		private const string MaxAuthenticationTimePropertyName = "maxAuthenticationTime";

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
			get { return (TimeSpan)this[MaxAuthenticationTimePropertyName]; }
			set { this[MaxAuthenticationTimePropertyName] = value; }
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
	}
}
