//-----------------------------------------------------------------------
// <copyright file="XriResolverElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;

	/// <summary>
	/// Represents the &lt;xriResolver&gt; element in the host's .config file.
	/// </summary>
	internal class XriResolverElement : ConfigurationElement {
		/// <summary>
		/// Gets the name of the @enabled attribute.
		/// </summary>
		private const string EnabledAttributeName = "enabled";

		/// <summary>
		/// The default value for <see cref="Enabled"/>.
		/// </summary>
		private const bool EnabledDefaultValue = true;

		/// <summary>
		/// The name of the &lt;proxy&gt; sub-element.
		/// </summary>
		private const string ProxyElementName = "proxy";

		/// <summary>
		/// The default XRI proxy resolver to use.
		/// </summary>
		private static readonly HostNameElement ProxyDefault = new HostNameElement("xri.net");

		/// <summary>
		/// Initializes a new instance of the <see cref="XriResolverElement"/> class.
		/// </summary>
		internal XriResolverElement() {
		}

		/// <summary>
		/// Gets or sets a value indicating whether this XRI resolution is enabled.
		/// </summary>
		/// <value>The default value is <c>true</c>.</value>
		[ConfigurationProperty(EnabledAttributeName, DefaultValue = EnabledDefaultValue)]
		internal bool Enabled {
			get { return (bool)this[EnabledAttributeName]; }
			set { this[EnabledAttributeName] = value; }
		}

		/// <summary>
		/// Gets or sets the proxy to use for resolving XRIs.
		/// </summary>
		/// <value>The default value is "xri.net".</value>
		[ConfigurationProperty(ProxyElementName)]
		internal HostNameElement Proxy {
			get {
				var host = (HostNameElement)this[ProxyElementName] ?? ProxyDefault;
				return string.IsNullOrEmpty(host.Name.Trim()) ? ProxyDefault : host;
			}

			set {
				this[ProxyElementName] = value;
			}
		}
	}
}
