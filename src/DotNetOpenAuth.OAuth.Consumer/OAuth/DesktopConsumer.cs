//-----------------------------------------------------------------------
// <copyright file="DesktopConsumer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Threading;
	using System.Threading.Tasks;
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The URL to open a browser window to allow the user to provide authorization and the request token.
		/// </returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "Two results")]
		public async Task<Tuple<Uri, string>> RequestUserAuthorizationAsync(IDictionary<string, string> requestParameters, IDictionary<string, string> redirectParameters, CancellationToken cancellationToken = default(CancellationToken)) {
			var message = await this.PrepareRequestUserAuthorizationAsync(null, requestParameters, redirectParameters, cancellationToken);
			var response = await this.Channel.PrepareResponseAsync(message, cancellationToken);
			return Tuple.Create(response.GetDirectUriRequest(), message.RequestToken);
		}

		/// <summary>
		/// Exchanges a given request token for access token.
		/// </summary>
		/// <param name="requestToken">The request token that the user has authorized.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The access token assigned by the Service Provider.</returns>
		[Obsolete("Use the ProcessUserAuthorization method that takes a verifier parameter instead.")]
		public Task<AuthorizedTokenResponse> ProcessUserAuthorizationAsync(string requestToken, CancellationToken cancellationToken = default(CancellationToken)) {
			return this.ProcessUserAuthorizationAsync(requestToken, null, cancellationToken);
		}

		/// <summary>
		/// Exchanges a given request token for access token.
		/// </summary>
		/// <param name="requestToken">The request token that the user has authorized.</param>
		/// <param name="verifier">The verifier code typed in by the user.  Must not be <c>Null</c> for OAuth 1.0a service providers and later.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The access token assigned by the Service Provider.
		/// </returns>
		public new Task<AuthorizedTokenResponse> ProcessUserAuthorizationAsync(string requestToken, string verifier, CancellationToken cancellationToken = default(CancellationToken)) {
			if (this.ServiceProvider.Version >= Protocol.V10a.Version) {
				ErrorUtilities.VerifyNonZeroLength(verifier, "verifier");
			}

			return base.ProcessUserAuthorizationAsync(requestToken, verifier, cancellationToken);
		}
	}
}
