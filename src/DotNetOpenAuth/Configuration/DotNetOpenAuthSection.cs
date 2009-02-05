//-----------------------------------------------------------------------
// <copyright file="DotNetOpenAuthSection.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;

	/// <summary>
	/// Represents the section in the host's .config file that configures
	/// this library's settings.
	/// </summary>
	public class DotNetOpenAuthSection : ConfigurationSection {
		/// <summary>
		/// The name of the section under which this library's settings must be found.
		/// </summary>
		private const string SectionName = "dotNetOpenAuth";

		/// <summary>
		/// The name of the &lt;messaging&gt; sub-element.
		/// </summary>
		private const string MessagingElementName = "messaging";

		/// <summary>
		/// The name of the &lt;openid&gt; sub-element.
		/// </summary>
		private const string OpenIdElementName = "openid";

		/// <summary>
		/// Initializes a new instance of the <see cref="DotNetOpenAuthSection"/> class.
		/// </summary>
		internal DotNetOpenAuthSection() {
			SectionInformation.AllowLocation = false;
		}

		/// <summary>
		/// Gets the configuration section from the .config file.
		/// </summary>
		public static DotNetOpenAuthSection Configuration {
			get { return (DotNetOpenAuthSection)ConfigurationManager.GetSection(SectionName) ?? new DotNetOpenAuthSection(); }
		}

		/// <summary>
		/// Gets or sets the configuration for the messaging framework.
		/// </summary>
		[ConfigurationProperty(MessagingElementName)]
		public MessagingElement Messaging {
			get { return (MessagingElement)this[MessagingElementName] ?? new MessagingElement(); }
			set { this[MessagingElementName] = value; }
		}

		/// <summary>
		/// Gets or sets the configuration for OpenID.
		/// </summary>
		[ConfigurationProperty(OpenIdElementName)]
		internal OpenIdElement OpenId {
			get { return (OpenIdElement)this[OpenIdElementName] ?? new OpenIdElement(); }
			set { this[OpenIdElementName] = value; }
		}
	}
}
