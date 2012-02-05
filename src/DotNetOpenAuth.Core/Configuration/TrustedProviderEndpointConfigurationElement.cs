//-----------------------------------------------------------------------
// <copyright file="TrustedProviderEndpointConfigurationElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;

	/// <summary>
	/// A configuration element that records a trusted Provider Endpoint.
	/// </summary>
	internal class TrustedProviderEndpointConfigurationElement : ConfigurationElement {
		/// <summary>
		/// The name of the attribute that stores the <see cref="ProviderEndpoint"/> value.
		/// </summary>
		private const string ProviderEndpointConfigName = "endpoint";

		/// <summary>
		/// Initializes a new instance of the <see cref="TrustedProviderEndpointConfigurationElement"/> class.
		/// </summary>
		public TrustedProviderEndpointConfigurationElement() {
		}

		/// <summary>
		/// Gets or sets the OpenID Provider Endpoint (aka "OP Endpoint") that this relying party trusts.
		/// </summary>
		[ConfigurationProperty(ProviderEndpointConfigName, IsRequired = true, IsKey = true)]
		public Uri ProviderEndpoint {
			get { return (Uri)this[ProviderEndpointConfigName]; }
			set { this[ProviderEndpointConfigName] = value; }
		}
	}
}
