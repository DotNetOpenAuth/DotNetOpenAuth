//-----------------------------------------------------------------------
// <copyright file="HostMetaDiscoveryElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;

	/// <summary>
	/// The configuration element that can adjust how hostmeta discovery works.
	/// </summary>
	internal class HostMetaDiscoveryElement : ConfigurationElement {
		/// <summary>
		/// The property name for enableCertificateValidationCache.
		/// </summary>
		private const string EnableCertificateValidationCacheConfigName = "enableCertificateValidationCache";

		/// <summary>
		/// Initializes a new instance of the <see cref="HostMetaDiscoveryElement"/> class.
		/// </summary>
		public HostMetaDiscoveryElement() {
		}

		/// <summary>
		/// Gets or sets a value indicating whether validated certificates should be cached and not validated again.
		/// </summary>
		/// <remarks>
		/// This helps to avoid unexplained 5-10 second delays in certificate validation for Google Apps for Domains that impact some servers.
		/// </remarks>
		[ConfigurationProperty(EnableCertificateValidationCacheConfigName, DefaultValue = false)]
		public bool EnableCertificateValidationCache {
			get { return (bool)this[EnableCertificateValidationCacheConfigName]; }
			set { this[EnableCertificateValidationCacheConfigName] = value; }
		}
	}
}
