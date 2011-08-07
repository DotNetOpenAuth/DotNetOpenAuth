//-----------------------------------------------------------------------
// <copyright file="DotNetOpenAuthSection.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// Represents the section in the host's .config file that configures
	/// this library's settings.
	/// </summary>
	[ContractVerification(true)]
	public class DotNetOpenAuthSection : ConfigurationSectionGroup {
		/// <summary>
		/// The name of the section under which this library's settings must be found.
		/// </summary>
		internal const string SectionName = "dotNetOpenAuth";

		/// <summary>
		/// The name of the &lt;openid&gt; sub-element.
		/// </summary>
		private const string OpenIdElementName = "openid";

		/// <summary>
		/// The name of the &lt;oauth&gt; sub-element.
		/// </summary>
		private const string OAuthElementName = "oauth";

		/// <summary>
		/// Initializes a new instance of the <see cref="DotNetOpenAuthSection"/> class.
		/// </summary>
		internal DotNetOpenAuthSection() {
		}

		/// <summary>
		/// Gets the configuration section from the .config file.
		/// </summary>
		public static DotNetOpenAuthSection Configuration {
			get {
				Contract.Ensures(Contract.Result<DotNetOpenAuthSection>() != null);
				return (DotNetOpenAuthSection)ConfigurationManager.GetSection(SectionName) ?? new DotNetOpenAuthSection();
			}
		}

		/// <summary>
		/// Gets the messaging configuration element.
		/// </summary>
		public static MessagingElement Messaging {
			get { return MessagingElement.Configuration; }
		}

		/// <summary>
		/// Gets the reporting configuration element.
		/// </summary>
		internal static ReportingElement Reporting {
			get { return ReportingElement.Configuration; }
		}

		/// <summary>
		/// Gets or sets the embedded resource retrieval provider.
		/// </summary>
		/// <value>
		/// The embedded resource retrieval provider.
		/// </value>
		internal TypeConfigurationElement<IEmbeddedResourceRetrieval> EmbeddedResourceRetrievalProvider {
			get { return /*TODO: (TypeConfigurationElement<IEmbeddedResourceRetrieval>)this[WebResourceUrlProviderName] ??*/ new TypeConfigurationElement<IEmbeddedResourceRetrieval>(); }
			set { /*this[WebResourceUrlProviderName] = value;*/ }
		}
	}
}
