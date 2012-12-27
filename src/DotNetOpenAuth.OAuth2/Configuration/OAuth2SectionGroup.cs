//-----------------------------------------------------------------------
// <copyright file="OAuth2SectionGroup.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;

	/// <summary>
	/// Represents the &lt;oauth&gt; element in the host's .config file.
	/// </summary>
	internal class OAuth2SectionGroup : ConfigurationSectionGroup {
		/// <summary>
		/// The name of the oauth section.
		/// </summary>
		internal const string SectionName = DotNetOpenAuthSection.SectionName + "/oauth2";

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2SectionGroup"/> class.
		/// </summary>
		internal OAuth2SectionGroup() {
		}
	}
}
