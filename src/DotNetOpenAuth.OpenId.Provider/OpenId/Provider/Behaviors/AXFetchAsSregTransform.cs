//-----------------------------------------------------------------------
// <copyright file="AXFetchAsSregTransform.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider.Behaviors {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Behaviors;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.Provider.Extensions;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// An Attribute Exchange and Simple Registration filter to make all incoming attribute 
	/// requests look like Simple Registration requests, and to convert the response
	/// to the originally requested extension and format.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sreg", Justification = "Abbreviation")]
	public sealed class AXFetchAsSregTransform : AXFetchAsSregTransformBase, IProviderBehavior {
		/// <summary>
		/// Initializes a new instance of the <see cref="AXFetchAsSregTransform"/> class.
		/// </summary>
		public AXFetchAsSregTransform() {
		}

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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		///   <c>true</c> if this behavior owns this request and wants to stop other behaviors
		/// from handling it; <c>false</c> to allow other behaviors to process this request.
		/// </returns>
		/// <remarks>
		/// Implementations may set a new value to <see cref="IRequest.SecuritySettings" /> but
		/// should not change the properties on the instance of <see cref="ProviderSecuritySettings" />
		/// itself as that instance may be shared across many requests.
		/// </remarks>
		Task<bool> IProviderBehavior.OnIncomingRequestAsync(IRequest request, CancellationToken cancellationToken) {
			var extensionRequest = request as Provider.HostProcessedRequest;
			if (extensionRequest != null) {
				extensionRequest.UnifyExtensionsAsSreg();
			}

			return Task.FromResult(false);
		}

		/// <summary>
		/// Called when the Provider is preparing to send a response to an authentication request.
		/// </summary>
		/// <param name="request">The request that is configured to generate the outgoing response.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		///   <c>true</c> if this behavior owns this request and wants to stop other behaviors
		/// from handling it; <c>false</c> to allow other behaviors to process this request.
		/// </returns>
		async Task<bool> IProviderBehavior.OnOutgoingResponseAsync(Provider.IAuthenticationRequest request, CancellationToken cancellationToken) {
			await request.ConvertSregToMatchRequestAsync(cancellationToken);
			return false;
		}

		#endregion
	}
}
