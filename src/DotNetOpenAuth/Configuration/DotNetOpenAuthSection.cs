//-----------------------------------------------------------------------
// <copyright file="DotNetOpenAuthSection.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// Represents the section in the host's .config file that configures
	/// this library's settings.
	/// </summary>
	[ContractVerification(true)]
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
		/// The name of the &lt;oauth&gt; sub-element.
		/// </summary>
		private const string OAuthElementName = "oauth";

		/// <summary>
		/// Initializes a new instance of the <see cref="DotNetOpenAuthSection"/> class.
		/// </summary>
		internal DotNetOpenAuthSection() {
			Contract.Assume(this.SectionInformation != null);
			this.SectionInformation.AllowLocation = false;
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

		/// <summary>
		/// Gets or sets the configuration for OAuth.
		/// </summary>
		[ConfigurationProperty(OAuthElementName)]
		internal OAuthElement OAuth {
			get { return (OAuthElement)this[OAuthElementName] ?? new OAuthElement(); }
			set { this[OAuthElementName] = value; }
		}
	}
}
