//-----------------------------------------------------------------------
// <copyright file="MessagingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// Represents the &lt;messaging&gt; element in the host's .config file.
	/// </summary>
	public class MessagingElement : ConfigurationElement {
		/// <summary>
		/// The name of the &lt;untrustedWebRequest&gt; sub-element.
		/// </summary>
		private const string UntrustedWebRequestElementName = "untrustedWebRequest";

		/// <summary>
		/// The name of the attribute that stores the association's maximum lifetime.
		/// </summary>
		private const string MaximumMessageLifetimeConfigName = "lifetime";

		/// <summary>
		/// Gets or sets the time between a message's creation and its receipt
		/// before it is considered expired.
		/// </summary>
		/// <value>
		/// The default value value is 13 minutes.
		/// </value>
		/// <remarks>
		/// 	<para>Smaller timespans mean lower tolerance for delays in message delivery
		/// and time variance due to server clocks not being synchronized.
		/// Larger timespans mean more nonces must be stored to provide replay protection.</para>
		/// 	<para>The maximum age a message implementing the
		/// <see cref="IExpiringProtocolMessage"/> interface can be before
		/// being discarded as too old.</para>
		/// 	<para>This time limit should take into account expected time skew for servers
		/// across the Internet.  For example, if a server could conceivably have its
		/// clock d = 5 minutes off UTC time, then any two servers could have
		/// their clocks disagree by as much as 2*d = 10 minutes.
		/// If a message should live for at least t = 3 minutes,
		/// this property should be set to (2*d + t) = 13 minutes.</para>
		/// </remarks>
		[ConfigurationProperty(MaximumMessageLifetimeConfigName, DefaultValue = "00:13:00")]
		public TimeSpan MaximumMessageLifetime {
			get { return (TimeSpan)this[MaximumMessageLifetimeConfigName]; }
			set { this[MaximumMessageLifetimeConfigName] = value; }
		}

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
