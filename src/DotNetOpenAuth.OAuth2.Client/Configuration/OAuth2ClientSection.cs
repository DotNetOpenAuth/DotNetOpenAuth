//-----------------------------------------------------------------------
// <copyright file="OAuth2ClientSection.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// Represents the &lt;oauth2/client&gt; section in the host's .config file.
	/// </summary>
	internal class OAuth2ClientSection : ConfigurationSection {
		/// <summary>
		/// The name of the oauth2/client section.
		/// </summary>
		private const string SectionName = OAuth2SectionGroup.SectionName + "/client";

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2ClientSection"/> class.
		/// </summary>
		internal OAuth2ClientSection() {
		}

		/// <summary>
		/// Gets the configuration section from the .config file.
		/// </summary>
		internal static OAuth2ClientSection Configuration {
			get {
				Contract.Ensures(Contract.Result<OAuth2ClientSection>() != null);
				return (OAuth2ClientSection)ConfigurationManager.GetSection(SectionName) ?? new OAuth2ClientSection();
			}
		}
	}
}
