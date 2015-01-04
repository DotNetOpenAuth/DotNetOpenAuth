//-----------------------------------------------------------------------
// <copyright file="GsaIcamProfileBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Behaviors {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

	/// <summary>
	/// Implements the Identity, Credential, &amp; Access Management (ICAM) OpenID 2.0 Profile
	/// for the General Services Administration (GSA).
	/// </summary>
	/// <remarks>
	/// <para>Relying parties that include this profile are always held to the terms required by the profile,
	/// but Providers are only affected by the special behaviors of the profile when the RP specifically
	/// indicates that they want to use this profile. </para>
	/// </remarks>
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Icam", Justification = "Acronym")]
	public abstract class GsaIcamProfileBase {
		/// <summary>
		/// Backing field for the <see cref="DisableSslRequirement"/> static property.
		/// </summary>
		private static bool disableSslRequirement = DotNetOpenAuthSection.Messaging.RelaxSslRequirements;

		/// <summary>
		/// Initializes a new instance of the <see cref="GsaIcamProfileBase"/> class.
		/// </summary>
		public GsaIcamProfileBase() {
			if (DisableSslRequirement) {
				Logger.OpenId.Warn("GSA level 1 behavior has its RequireSsl requirement disabled.");
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether PII is allowed to be requested or received via OpenID.
		/// </summary>
		/// <value>The default value is <c>false</c>.</value>
		public static bool AllowPersonallyIdentifiableInformation { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to ignore the SSL requirement (for testing purposes only).
		/// </summary>
		public static bool DisableSslRequirement { // not an auto-property because it has a default value, and FxCop doesn't want us using static constructors.
			get { return disableSslRequirement; }
			set { disableSslRequirement = value; }
		}
	}
}
