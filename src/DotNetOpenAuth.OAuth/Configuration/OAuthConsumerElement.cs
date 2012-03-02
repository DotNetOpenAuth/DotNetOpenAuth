//-----------------------------------------------------------------------
// <copyright file="OAuthConsumerElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;

	/// <summary>
	/// Represents the &lt;oauth/consumer&gt; element in the host's .config file.
	/// </summary>
	internal class OAuthConsumerElement : ConfigurationElement {
		/// <summary>
		/// Gets the name of the security sub-element.
		/// </summary>
		private const string SecuritySettingsConfigName = "security";

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthConsumerElement"/> class.
		/// </summary>
		internal OAuthConsumerElement() {
		}

		/// <summary>
		/// Gets or sets the security settings.
		/// </summary>
		[ConfigurationProperty(SecuritySettingsConfigName)]
		public OAuthConsumerSecuritySettingsElement SecuritySettings {
			get { return (OAuthConsumerSecuritySettingsElement)this[SecuritySettingsConfigName] ?? new OAuthConsumerSecuritySettingsElement(); }
			set { this[SecuritySettingsConfigName] = value; }
		}
	}
}
