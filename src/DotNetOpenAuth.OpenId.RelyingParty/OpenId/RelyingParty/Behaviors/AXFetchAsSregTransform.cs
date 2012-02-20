//-----------------------------------------------------------------------
// <copyright file="AXFetchAsSregTransform.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty.Behaviors {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Behaviors;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.OpenId.RelyingParty.Extensions;

	/// <summary>
	/// An Attribute Exchange and Simple Registration filter to make all incoming attribute 
	/// requests look like Simple Registration requests, and to convert the response
	/// to the originally requested extension and format.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sreg", Justification = "Abbreviation")]
	public sealed class AXFetchAsSregTransform : AXFetchAsSregTransformBase, IRelyingPartyBehavior {
		/// <summary>
		/// Initializes a new instance of the <see cref="AXFetchAsSregTransform"/> class.
		/// </summary>
		public AXFetchAsSregTransform() {
		}

		#region IRelyingPartyBehavior Members

		/// <summary>
		/// Applies a well known set of security requirements to a default set of security settings.
		/// </summary>
		/// <param name="securitySettings">The security settings to enhance with the requirements of this profile.</param>
		/// <remarks>
		/// Care should be taken to never decrease security when applying a profile.
		/// Profiles should only enhance security requirements to avoid being
		/// incompatible with each other.
		/// </remarks>
		void IRelyingPartyBehavior.ApplySecuritySettings(RelyingPartySecuritySettings securitySettings) {
		}

		/// <summary>
		/// Called when an authentication request is about to be sent.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <remarks>
		/// Implementations should be prepared to be called multiple times on the same outgoing message
		/// without malfunctioning.
		/// </remarks>
		void IRelyingPartyBehavior.OnOutgoingAuthenticationRequest(RelyingParty.IAuthenticationRequest request) {
			// Don't create AX extensions for OpenID 1.x messages, since AX requires OpenID 2.0.
			if (request.Provider.Version.Major >= 2) {
				request.SpreadSregToAX(this.AXFormats);
			}
		}

		/// <summary>
		/// Called when an incoming positive assertion is received.
		/// </summary>
		/// <param name="assertion">The positive assertion.</param>
		void IRelyingPartyBehavior.OnIncomingPositiveAssertion(IAuthenticationResponse assertion) {
			if (assertion.GetExtension<ClaimsResponse>() == null) {
				ClaimsResponse sreg = assertion.UnifyExtensionsAsSreg(true);
				((PositiveAnonymousResponse)assertion).Response.Extensions.Add(sreg);
			}
		}

		#endregion
	}
}
