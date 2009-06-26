//-----------------------------------------------------------------------
// <copyright file="IProviderBehavior.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using DotNetOpenAuth.OpenId.ChannelElements;

	/// <summary>
	/// Applies a custom security policy to certain OpenID security settings and behaviors.
	/// </summary>
	/// <remarks>
	/// BEFORE MARKING THIS INTERFACE PUBLIC: it's very important that we shift the methods to be channel-level
	/// rather than facade class level and for the OpenIdChannel to be the one to invoke these methods.
	/// </remarks>
	internal interface IProviderBehavior {
		/// <summary>
		/// Applies a well known set of security requirements to a default set of security settings.
		/// </summary>
		/// <param name="securitySettings">The security settings to enhance with the requirements of this profile.</param>
		/// <remarks>
		/// Care should be taken to never decrease security when applying a profile.
		/// Profiles should only enhance security requirements to avoid being
		/// incompatible with each other.
		/// </remarks>
		void ApplySecuritySettings(ProviderSecuritySettings securitySettings);

		/// <summary>
		/// Called when a request is received by the Provider.
		/// </summary>
		/// <param name="request">The incoming request.</param>
		/// <returns>
		/// <c>true</c> if this behavior owns this request and wants to stop other behaviors
		/// from handling it; <c>false</c> to allow other behaviors to process this request.
		/// </returns>
		/// <remarks>
		/// Implementations may set a new value to <see cref="IRequest.SecuritySettings"/> but
		/// should not change the properties on the instance of <see cref="ProviderSecuritySettings"/>
		/// itself as that instance may be shared across many requests.
		/// </remarks>
		bool OnIncomingRequest(IRequest request);

		/// <summary>
		/// Called when the Provider is preparing to send a response to an authentication request.
		/// </summary>
		/// <param name="request">The request that is configured to generate the outgoing response.</param>
		/// <returns>
		/// <c>true</c> if this behavior owns this request and wants to stop other behaviors
		/// from handling it; <c>false</c> to allow other behaviors to process this request.
		/// </returns>
		bool OnOutgoingResponse(IAuthenticationRequest request);
	}
}
