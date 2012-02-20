//-----------------------------------------------------------------------
// <copyright file="OAuthConsumerSecuritySettingsElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth;

	/// <summary>
	/// Security settings that are applicable to consumers.
	/// </summary>
	internal class OAuthConsumerSecuritySettingsElement : ConfigurationElement {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthConsumerSecuritySettingsElement"/> class.
		/// </summary>
		internal OAuthConsumerSecuritySettingsElement() {
		}

		/// <summary>
		/// Initializes a programmatically manipulatable bag of these security settings with the settings from the config file.
		/// </summary>
		/// <returns>The newly created security settings object.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "By design")]
		internal ConsumerSecuritySettings CreateSecuritySettings() {
			return new ConsumerSecuritySettings();
		}
	}
}
