//-----------------------------------------------------------------------
// <copyright file="OAuth2ClientElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;

	/// <summary>
	/// Represents the &lt;oauth2/client&gt; element in the host's .config file.
	/// </summary>
	internal class OAuth2ClientElement : ConfigurationElement {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2ClientElement"/> class.
		/// </summary>
		internal OAuth2ClientElement() {
		}
	}
}
