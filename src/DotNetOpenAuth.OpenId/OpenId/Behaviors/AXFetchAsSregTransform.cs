//-----------------------------------------------------------------------
// <copyright file="AXFetchAsSregTransform.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Behaviors {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// An Attribute Exchange and Simple Registration filter to make all incoming attribute 
	/// requests look like Simple Registration requests, and to convert the response
	/// to the originally requested extension and format.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sreg", Justification = "Abbreviation")]
	public sealed class AXFetchAsSregTransform : IRelyingPartyBehavior, IProviderBehavior {
		/// <summary>
		/// Initializes static members of the <see cref="AXFetchAsSregTransform"/> class.
		/// </summary>
		static AXFetchAsSregTransform() {
			AXFormats = AXAttributeFormats.Common;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AXFetchAsSregTransform"/> class.
		/// </summary>
		public AXFetchAsSregTransform() {
		}

		/// <summary>
		/// Gets or sets the AX attribute type URI formats this transform is willing to work with.
		/// </summary>
		public static AXAttributeFormats AXFormats { get; set; }

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
				request.SpreadSregToAX(AXFormats);
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

		#region IProviderBehavior Members

		/// <summary>
		/// Applies a well known set of security requirements to a default set of security settings.
		/// </summary>
		/// <param name="securitySettings">The security settings to enhance with the requirements of this profile.</param>
		/// <remarks>
		/// Care should be taken to never decrease security when applying a profile.
		/// Profiles should only enhance security requirements to avoid being
		/// incompatible with each other.
		/// </remarks>
		void IProviderBehavior.ApplySecuritySettings(ProviderSecuritySettings securitySettings) {
			// Nothing to do here.
		}

		/// <summary>
		/// Called when a request is received by the Provider.
		/// </summary>
		/// <param name="request">The incoming request.</param>
		/// <returns>
		/// 	<c>true</c> if this behavior owns this request and wants to stop other behaviors
		/// from handling it; <c>false</c> to allow other behaviors to process this request.
		/// </returns>
		/// <remarks>
		/// Implementations may set a new value to <see cref="IRequest.SecuritySettings"/> but
		/// should not change the properties on the instance of <see cref="ProviderSecuritySettings"/>
		/// itself as that instance may be shared across many requests.
		/// </remarks>
		bool IProviderBehavior.OnIncomingRequest(IRequest request) {
			var extensionRequest = request as Provider.HostProcessedRequest;
			if (extensionRequest != null) {
				extensionRequest.UnifyExtensionsAsSreg();
			}

			return false;
		}

		/// <summary>
		/// Called when the Provider is preparing to send a response to an authentication request.
		/// </summary>
		/// <param name="request">The request that is configured to generate the outgoing response.</param>
		/// <returns>
		/// 	<c>true</c> if this behavior owns this request and wants to stop other behaviors
		/// from handling it; <c>false</c> to allow other behaviors to process this request.
		/// </returns>
		bool IProviderBehavior.OnOutgoingResponse(Provider.IAuthenticationRequest request) {
			request.ConvertSregToMatchRequest();
			return false;
		}

		#endregion
	}
}
