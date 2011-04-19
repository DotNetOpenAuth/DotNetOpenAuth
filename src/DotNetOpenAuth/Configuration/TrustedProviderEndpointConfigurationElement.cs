//-----------------------------------------------------------------------
// <copyright file="TrustedProviderEndpoint.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
		/// The name of the attribute that stores the <see cref="AllowSubPath"/> value.
		/// </summary>
		private const string AllowSubPathConfigName = "allowSubPath";

		/// <summary>
		/// The name of the attribute that stores the <see cref="AllowAdditionalQueryParameters"/> value.
		/// </summary>
		private const string AllowAdditionalQueryParametersConfigName = "allowAdditionalQueryParameters";

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

		/// <summary>
		/// Gets or sets a value indicating whether the OP Endpoint given here is a base path, and sub-paths concatenated to it are equally trusted.
		/// </summary>
		[ConfigurationProperty(AllowSubPathConfigName, DefaultValue = false)]
		public bool AllowSubPath {
			get { return (bool)this[AllowSubPathConfigName]; }
			set { this[AllowSubPathConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the OP Endpoint given here is equally trusted if query string parameters are added to it.
		/// </summary>
		[ConfigurationProperty(AllowAdditionalQueryParametersConfigName, DefaultValue = false)]
		public bool AllowAdditionalQueryParameters {
			get { return (bool)this[AllowAdditionalQueryParametersConfigName]; }
			set { this[AllowAdditionalQueryParametersConfigName] = value; }
		}
	}
}
