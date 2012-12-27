//-----------------------------------------------------------------------
// <copyright file="OAuthElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;

	/// <summary>
	/// Represents the &lt;oauth&gt; element in the host's .config file.
	/// </summary>
	internal class OAuthElement : ConfigurationSection {
		/// <summary>
		/// The name of the oauth section.
		/// </summary>
		private const string SectionName = DotNetOpenAuthSection.SectionName + "/oauth";

		/// <summary>
		/// The name of the &lt;consumer&gt; sub-element.
		/// </summary>
		private const string ConsumerElementName = "consumer";

		/// <summary>
		/// The name of the &lt;serviceProvider&gt; sub-element.
		/// </summary>
		private const string ServiceProviderElementName = "serviceProvider";

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthElement"/> class.
		/// </summary>
		internal OAuthElement() {
		}

		/// <summary>
		/// Gets the configuration section from the .config file.
		/// </summary>
		public static OAuthElement Configuration {
			get {
				return (OAuthElement)ConfigurationManager.GetSection(SectionName) ?? new OAuthElement();
			}
		}

		/// <summary>
		/// Gets or sets the configuration specific for Consumers.
		/// </summary>
		[ConfigurationProperty(ConsumerElementName)]
		internal OAuthConsumerElement Consumer {
			get { return (OAuthConsumerElement)this[ConsumerElementName] ?? new OAuthConsumerElement(); }
			set { this[ConsumerElementName] = value; }
		}

		/// <summary>
		/// Gets or sets the configuration specific for Service Providers.
		/// </summary>
		[ConfigurationProperty(ServiceProviderElementName)]
		internal OAuthServiceProviderElement ServiceProvider {
			get { return (OAuthServiceProviderElement)this[ServiceProviderElementName] ?? new OAuthServiceProviderElement(); }
			set { this[ServiceProviderElementName] = value; }
		}
	}
}
