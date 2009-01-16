//-----------------------------------------------------------------------
// <copyright file="MessagingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Represents the &lt;messaging&gt; element in the host's .config file.
	/// </summary>
	internal class MessagingElement : ConfigurationElement {
		/// <summary>
		/// The name of the &lt;untrustedWebRequest&gt; sub-element.
		/// </summary>
		private const string UntrustedWebRequestElementName = "untrustedWebRequest";

		/// <summary>
		/// Gets or sets the configuration for the <see cref="UntrustedWebRequestHandler"/> class.
		/// </summary>
		/// <value>The untrusted web request.</value>
		[ConfigurationProperty(UntrustedWebRequestElementName)]
		internal UntrustedWebRequestElement UntrustedWebRequest {
			get { return (UntrustedWebRequestElement)this[UntrustedWebRequestElementName] ?? new UntrustedWebRequestElement(); }
			set { this[UntrustedWebRequestElementName] = value; }
		}
	}
}
