//-----------------------------------------------------------------------
// <copyright file="TrustedProviderConfigurationCollection.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// A configuration collection of trusted OP Endpoints.
	/// </summary>
	internal class TrustedProviderConfigurationCollection : ConfigurationElementCollection {
		/// <summary>
		/// The name of the "rejectAssertionsFromUntrustedProviders" element.
		/// </summary>
		private const string RejectAssertionsFromUntrustedProvidersConfigName = "rejectAssertionsFromUntrustedProviders";

		/// <summary>
		/// Initializes a new instance of the <see cref="TrustedProviderConfigurationCollection"/> class.
		/// </summary>
		internal TrustedProviderConfigurationCollection() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TrustedProviderConfigurationCollection"/> class.
		/// </summary>
		/// <param name="elements">The elements to initialize the collection with.</param>
		internal TrustedProviderConfigurationCollection(IEnumerable<TrustedProviderEndpointConfigurationElement> elements) {
			Contract.Requires<ArgumentNullException>(elements != null);

			foreach (TrustedProviderEndpointConfigurationElement element in elements) {
				this.BaseAdd(element);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether any login attempt coming from an OpenID Provider Endpoint that is not on this
		/// whitelist of trusted OP Endpoints will be rejected.  If the trusted providers list is empty and this value
		/// is true, all assertions are rejected.
		/// </summary>
		[ConfigurationProperty(RejectAssertionsFromUntrustedProvidersConfigName, DefaultValue = false)]
		internal bool RejectAssertionsFromUntrustedProviders {
			get { return (bool)this[RejectAssertionsFromUntrustedProvidersConfigName]; }
			set { this[RejectAssertionsFromUntrustedProvidersConfigName] = value; }
		}

		/// <summary>
		/// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"/>.
		/// </summary>
		/// <returns>
		/// A new <see cref="T:System.Configuration.ConfigurationElement"/>.
		/// </returns>
		protected override ConfigurationElement CreateNewElement() {
			return new TrustedProviderEndpointConfigurationElement();
		}

		/// <summary>
		/// Gets the element key for a specified configuration element when overridden in a derived class.
		/// </summary>
		/// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
		/// </returns>
		protected override object GetElementKey(ConfigurationElement element) {
			return ((TrustedProviderEndpointConfigurationElement)element).ProviderEndpoint;
		}
	}
}
