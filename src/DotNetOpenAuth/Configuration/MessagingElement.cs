//-----------------------------------------------------------------------
// <copyright file="MessagingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// Represents the &lt;messaging&gt; element in the host's .config file.
	/// </summary>
	[ContractVerification(true)]
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
		/// The name of the attribute that stores the maximum allowable clock skew.
		/// </summary>
		private const string MaximumClockSkewConfigName = "clockSkew";

		/// <summary>
		/// Gets the actual maximum message lifetime that a program should allow.
		/// </summary>
		/// <value>The sum of the <see cref="MaximumMessageLifetime"/> and 
		/// <see cref="MaximumClockSkew"/> property values.</value>
		public TimeSpan MaximumMessageLifetime {
			get { return this.MaximumMessageLifetimeNoSkew + this.MaximumClockSkew; }
		}

		/// <summary>
		/// Gets or sets the time between a message's creation and its receipt
		/// before it is considered expired.
		/// </summary>
		/// <value>
		/// The default value value is 3 minutes.
		/// </value>
		/// <remarks>
		/// 	<para>Smaller timespans mean lower tolerance for delays in message delivery.
		/// Larger timespans mean more nonces must be stored to provide replay protection.</para>
		/// 	<para>The maximum age a message implementing the
		/// <see cref="IExpiringProtocolMessage"/> interface can be before
		/// being discarded as too old.</para>
		/// 	<para>This time limit should NOT take into account expected 
		/// time skew for servers across the Internet.  Time skew is added to
		/// this value and is controlled by the <see cref="MaximumClockSkew"/> property.</para>
		/// </remarks>
		[ConfigurationProperty(MaximumMessageLifetimeConfigName, DefaultValue = "00:03:00")]
		internal TimeSpan MaximumMessageLifetimeNoSkew {
			get { return (TimeSpan)this[MaximumMessageLifetimeConfigName]; }
			set { this[MaximumMessageLifetimeConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the maximum clock skew.
		/// </summary>
		/// <value>The default value is 10 minutes.</value>
		/// <remarks>
		/// 	<para>Smaller timespans mean lower tolerance for 
		/// time variance due to server clocks not being synchronized.
		/// Larger timespans mean greater chance for replay attacks and
		/// larger nonce caches.</para>
		/// 	<para>For example, if a server could conceivably have its
		/// clock d = 5 minutes off UTC time, then any two servers could have
		/// their clocks disagree by as much as 2*d = 10 minutes. </para>
		/// </remarks>
		[ConfigurationProperty(MaximumClockSkewConfigName, DefaultValue = "00:10:00")]
		internal TimeSpan MaximumClockSkew {
			get { return (TimeSpan)this[MaximumClockSkewConfigName]; }
			set { this[MaximumClockSkewConfigName] = value; }
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
