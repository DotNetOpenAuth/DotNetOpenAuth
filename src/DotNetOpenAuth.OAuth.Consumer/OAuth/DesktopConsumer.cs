//-----------------------------------------------------------------------
// <copyright file="DesktopConsumer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// Used by a desktop application to use OAuth to access the Service Provider on behalf of the User.
	/// </summary>
	/// <remarks>
	/// The methods on this class are thread-safe.  Provided the properties are set and not changed
	/// afterward, a single instance of this class may be used by an entire desktop application safely.
	/// </remarks>
	public class DesktopConsumer : ConsumerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="DesktopConsumer"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior of the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		public DesktopConsumer(ServiceProviderDescription serviceDescription, IConsumerTokenManager tokenManager)
			: base(serviceDescription, tokenManager) {
		}

		/// <summary>
		/// Begins an OAuth authorization request.
		/// </summary>
		/// <param name="requestParameters">Extra parameters to add to the request token message.  Optional.</param>
		/// <param name="redirectParameters">Extra parameters to add to the redirect to Service Provider message.  Optional.</param>
		/// <param name="requestToken">The request token that must be exchanged for an access token after the user has provided authorization.</param>
		/// <returns>The URL to open a browser window to allow the user to provide authorization.</returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "Two results")]
		public Uri RequestUserAuthorization(IDictionary<string, string> requestParameters, IDictionary<string, string> redirectParameters, out string requestToken) {
			var message = this.PrepareRequestUserAuthorization(null, requestParameters, redirectParameters, out requestToken);
			OutgoingWebResponse response = this.Channel.PrepareResponse(message);
			return response.GetDirectUriRequest(this.Channel);
		}

		/// <summary>
		/// Exchanges a given request token for access token.
		/// </summary>
		/// <param name="requestToken">The request token that the user has authorized.</param>
		/// <returns>The access token assigned by the Service Provider.</returns>
		[Obsolete("Use the ProcessUserAuthorization method that takes a verifier parameter instead.")]
		public AuthorizedTokenResponse ProcessUserAuthorization(string requestToken) {
			return this.ProcessUserAuthorization(requestToken, null);
		}

		/// <summary>
		/// Exchanges a given request token for access token.
		/// </summary>
		/// <param name="requestToken">The request token that the user has authorized.</param>
		/// <param name="verifier">The verifier code typed in by the user.  Must not be <c>Null</c> for OAuth 1.0a service providers and later.</param>
		/// <returns>
		/// The access token assigned by the Service Provider.
		/// </returns>
		public new AuthorizedTokenResponse ProcessUserAuthorization(string requestToken, string verifier) {
			if (this.ServiceProvider.Version >= Protocol.V10a.Version) {
				ErrorUtilities.VerifyNonZeroLength(verifier, "verifier");
			}

			return base.ProcessUserAuthorization(requestToken, verifier);
		}
	}
}
