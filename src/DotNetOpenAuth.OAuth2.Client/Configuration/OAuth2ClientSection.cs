//-----------------------------------------------------------------------
// <copyright file="OAuth2ClientSection.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;
	using Validation;

	/// <summary>
	/// Represents the &lt;oauth2/client&gt; section in the host's .config file.
	/// </summary>
	internal class OAuth2ClientSection : ConfigurationSection {
		/// <summary>
		/// The name of the oauth2/client section.
		/// </summary>
		private const string SectionName = OAuth2SectionGroup.SectionName + "/client";

		/// <summary>
		/// The name of the @maxAuthorizationTime attribute.
		/// </summary>
		private const string MaxAuthorizationTimePropertyName = "maxAuthorizationTime";

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
				return (OAuth2ClientSection)ConfigurationManager.GetSection(SectionName) ?? new OAuth2ClientSection();
			}
		}

		/// <summary>
		/// Gets or sets the maximum time a user can take to complete authentication.
		/// </summary>
		[ConfigurationProperty(MaxAuthorizationTimePropertyName, DefaultValue = "0:15")] // 15 minutes
		[PositiveTimeSpanValidator]
		internal TimeSpan MaxAuthorizationTime {
			get {
				TimeSpan result = (TimeSpan)this[MaxAuthorizationTimePropertyName];
				Assumes.True(result > TimeSpan.Zero); // our PositiveTimeSpanValidator should take care of this
				return result;
			}

			set {
				Requires.Range(value > TimeSpan.Zero, "value");
				this[MaxAuthorizationTimePropertyName] = value;
			}
		}
	}
}
