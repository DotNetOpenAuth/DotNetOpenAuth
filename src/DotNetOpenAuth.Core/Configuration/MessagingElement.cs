//-----------------------------------------------------------------------
// <copyright file="MessagingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
	public class MessagingElement : ConfigurationSection {
		/// <summary>
		/// The name of the &lt;webResourceUrlProvider&gt; sub-element.
		/// </summary>
		private const string WebResourceUrlProviderName = "webResourceUrlProvider";

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
		/// The name of the attribute that indicates whether to disable SSL requirements across the library.
		/// </summary>
		private const string RelaxSslRequirementsConfigName = "relaxSslRequirements";

		/// <summary>
		/// The name of the attribute that controls whether messaging rules are strictly followed.
		/// </summary>
		private const string StrictConfigName = "strict";

		/// <summary>
		/// The default value for the <see cref="MaximumIndirectMessageUrlLength"/> property.
		/// </summary>
		/// <value>
		/// 2KB, recommended by OpenID group
		/// </value>
		private const int DefaultMaximumIndirectMessageUrlLength = 2 * 1024;

		/// <summary>
		/// The name of the attribute that controls the maximum length of a URL before it is converted
		/// to a POST payload.
		/// </summary>
		private const string MaximumIndirectMessageUrlLengthConfigName = "maximumIndirectMessageUrlLength";

		/// <summary>
		/// Gets the name of the @privateSecretMaximumAge attribute.
		/// </summary>
		private const string PrivateSecretMaximumAgeConfigName = "privateSecretMaximumAge";

		/// <summary>
		/// The name of the &lt;messaging&gt; sub-element.
		/// </summary>
		private const string MessagingElementName = DotNetOpenAuthSection.SectionName + "/messaging";

		/// <summary>
		/// Gets the configuration section from the .config file.
		/// </summary>
		public static MessagingElement Configuration {
			get {
				return (MessagingElement)ConfigurationManager.GetSection(MessagingElementName) ?? new MessagingElement();
			}
		}

		/// <summary>
		/// Gets the actual maximum message lifetime that a program should allow.
		/// </summary>
		/// <value>The sum of the <see cref="MaximumMessageLifetime"/> and 
		/// <see cref="MaximumClockSkew"/> property values.</value>
		public TimeSpan MaximumMessageLifetime {
			get { return this.MaximumMessageLifetimeNoSkew + this.MaximumClockSkew; }
		}

		/// <summary>
		/// Gets or sets the maximum lifetime of a private symmetric secret,
		/// that may be used for signing or encryption.
		/// </summary>
		/// <value>The default value is 28 days (twice the age of the longest association).</value>
		[ConfigurationProperty(PrivateSecretMaximumAgeConfigName, DefaultValue = "28.00:00:00")]
		public TimeSpan PrivateSecretMaximumAge {
			get { return (TimeSpan)this[PrivateSecretMaximumAgeConfigName]; }
			set { this[PrivateSecretMaximumAgeConfigName] = value; }
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
		/// Gets or sets a value indicating whether SSL requirements within the library are disabled/relaxed.
		/// Use for TESTING ONLY.
		/// </summary>
		[ConfigurationProperty(RelaxSslRequirementsConfigName, DefaultValue = false)]
		internal bool RelaxSslRequirements {
			get { return (bool)this[RelaxSslRequirementsConfigName]; }
			set { this[RelaxSslRequirementsConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether messaging rules are strictly
		/// adhered to.
		/// </summary>
		/// <value><c>true</c> by default.</value>
		/// <remarks>
		/// Strict will require that remote parties adhere strictly to the specifications,
		/// even when a loose interpretation would not compromise security.
		/// <c>true</c> is a good default because it shakes out interoperability bugs in remote services
		/// so they can be identified and corrected.  But some web sites want things to Just Work
		/// more than they want to file bugs against others, so <c>false</c> is the setting for them.
		/// </remarks>
		[ConfigurationProperty(StrictConfigName, DefaultValue = true)]
		internal bool Strict {
			get { return (bool)this[StrictConfigName]; }
			set { this[StrictConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the configuration for the UntrustedWebRequestHandler class.
		/// </summary>
		/// <value>The untrusted web request.</value>
		[ConfigurationProperty(UntrustedWebRequestElementName)]
		internal UntrustedWebRequestElement UntrustedWebRequest {
			get { return (UntrustedWebRequestElement)this[UntrustedWebRequestElementName] ?? new UntrustedWebRequestElement(); }
			set { this[UntrustedWebRequestElementName] = value; }
		}

		/// <summary>
		/// Gets or sets the maximum allowable size for a 301 Redirect response before we send
		/// a 200 OK response with a scripted form POST with the parameters instead
		/// in order to ensure successfully sending a large payload to another server
		/// that might have a maximum allowable size restriction on its GET request.
		/// </summary>
		/// <value>The default value is 2048.</value>
		[ConfigurationProperty(MaximumIndirectMessageUrlLengthConfigName, DefaultValue = DefaultMaximumIndirectMessageUrlLength)]
		[IntegerValidator(MinValue = 500, MaxValue = 4096)]
		internal int MaximumIndirectMessageUrlLength {
			get { return (int)this[MaximumIndirectMessageUrlLengthConfigName]; }
			set { this[MaximumIndirectMessageUrlLengthConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the embedded resource retrieval provider.
		/// </summary>
		/// <value>
		/// The embedded resource retrieval provider.
		/// </value>
		[ConfigurationProperty(WebResourceUrlProviderName)]
		internal TypeConfigurationElement<IEmbeddedResourceRetrieval> EmbeddedResourceRetrievalProvider {
			get { return (TypeConfigurationElement<IEmbeddedResourceRetrieval>)this[WebResourceUrlProviderName] ?? new TypeConfigurationElement<IEmbeddedResourceRetrieval>(); }
			set { this[WebResourceUrlProviderName] = value; }
		}
	}
}
