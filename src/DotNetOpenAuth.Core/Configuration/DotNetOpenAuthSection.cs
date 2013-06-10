//-----------------------------------------------------------------------
// <copyright file="DotNetOpenAuthSection.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;
	using System.Web;
	using System.Web.Configuration;

	/// <summary>
	/// Represents the section in the host's .config file that configures
	/// this library's settings.
	/// </summary>
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
		/// <param name="name">The name of the section to obtain.</param>
		/// <returns>The desired section, or null if it could not be obtained.</returns>
		internal static ConfigurationSection GetNamedSection(string name) {
			string fullyQualifiedSectionName = SectionName + "/" + name;
			if (HttpContext.Current != null) {
				return (ConfigurationSection)WebConfigurationManager.GetSection(fullyQualifiedSectionName);
			} else {
				var configuration = ConfigurationManager.OpenExeConfiguration(null);
				return configuration != null ? configuration.GetSection(fullyQualifiedSectionName) : null;
			}
		}
	}
}
