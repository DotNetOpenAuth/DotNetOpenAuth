//-----------------------------------------------------------------------
// <copyright file="DotNetOpenAuthSection.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;
	using System.Diagnostics.Contracts;
	using System.Web;
	using System.Web.Configuration;

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
		/// A value indicating whether this instance came from a real Configuration instance.
		/// </summary>
		private bool synthesizedInstance;

		/// <summary>
		/// Initializes a new instance of the <see cref="DotNetOpenAuthSection"/> class.
		/// </summary>
		internal DotNetOpenAuthSection() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DotNetOpenAuthSection"/> class.
		/// </summary>
		private DotNetOpenAuthSection(bool synthesized) {
			this.synthesizedInstance = synthesized;
		}

		/// <summary>
		/// Gets the configuration section from the .config file.
		/// </summary>
		public static DotNetOpenAuthSection Configuration {
			get {
				Contract.Ensures(Contract.Result<DotNetOpenAuthSection>() != null);
				Configuration configuration;
				if (HttpContext.Current != null) {
					configuration = HttpContext.Current.Request.ApplicationPath != null
						? WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath)
						: ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				} else {
					configuration = ConfigurationManager.OpenExeConfiguration(null);
				}

				return (DotNetOpenAuthSection)configuration.GetSectionGroup(SectionName) ?? new DotNetOpenAuthSection(true);
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
		/// Gets a named section in this section group, or <c>null</c> if no such section is defined.
		/// </summary>
		internal ConfigurationSection GetNamedSection(string name) {
			return this.synthesizedInstance ? null : this.Sections[name];
		}
	}
}
