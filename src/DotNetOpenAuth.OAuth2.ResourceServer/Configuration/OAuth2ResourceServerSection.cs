//-----------------------------------------------------------------------
// <copyright file="OAuth2ResourceServerSection.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;

	/// <summary>
	/// Represents the &lt;oauth2/resourceServer&gt; section in the host's .config file.
	/// </summary>
	internal class OAuth2ResourceServerSection : ConfigurationElement {
		/// <summary>
		/// The name of the oauth2/client section.
		/// </summary>
		private const string SectionName = OAuth2SectionGroup.SectionName + "/resourceServer";

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2ResourceServerSection"/> class.
		/// </summary>
		internal OAuth2ResourceServerSection() {
		}

		/// <summary>
		/// Gets the configuration section from the .config file.
		/// </summary>
		internal static OAuth2ResourceServerSection Configuration {
			get {
				return (OAuth2ResourceServerSection)ConfigurationManager.GetSection(SectionName) ?? new OAuth2ResourceServerSection();
			}
		}
	}
}
