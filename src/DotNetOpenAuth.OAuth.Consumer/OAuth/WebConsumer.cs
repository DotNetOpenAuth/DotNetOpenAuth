//-----------------------------------------------------------------------
// <copyright file="WebConsumer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using Validation;

	/// <summary>
	/// A website or application that uses OAuth to access the Service Provider on behalf of the User.
	/// </summary>
	/// <remarks>
	/// The methods on this class are thread-safe.  Provided the properties are set and not changed
	/// afterward, a single instance of this class may be used by an entire web application safely.
	/// </remarks>
	public class WebConsumer : ConsumerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebConsumer"/> class.
		/// </summary>
		/// <param name="serviceDescription">The endpoints and behavior of the Service Provider.</param>
		/// <param name="tokenManager">The host's method of storing and recalling tokens and secrets.</param>
		public WebConsumer(ServiceProviderDescription serviceDescription, IConsumerTokenManager tokenManager)
			: base(serviceDescription, tokenManager) {
		}

		/// <summary>
		/// Begins an OAuth authorization request and redirects the user to the Service Provider
		/// to provide that authorization.  Upon successful authorization, the user is redirected
		/// back to the current page.
		/// </summary>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public Task<UserAuthorizationRequest> PrepareRequestUserAuthorizationAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			Uri callback = this.Channel.GetRequestFromContext().GetPublicFacingUrl().StripQueryArgumentsWithPrefix(Protocol.ParameterPrefix);
			return this.PrepareRequestUserAuthorizationAsync(callback, null, null, cancellationToken);
		}

		/// <summary>
		/// Prepares an OAuth message that begins an authorization request that will 
		/// redirect the user to the Service Provider to provide that authorization.
		/// </summary>
		/// <param name="callback">
		/// An optional Consumer URL that the Service Provider should redirect the 
		/// User Agent to upon successful authorization.
		/// </param>
		/// <param name="requestParameters">Extra parameters to add to the request token message.  Optional.</param>
		/// <param name="redirectParameters">Extra parameters to add to the redirect to Service Provider message.  Optional.</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		public Task<UserAuthorizationRequest> PrepareRequestUserAuthorizationAsync(Uri callback, IDictionary<string, string> requestParameters, IDictionary<string, string> redirectParameters, CancellationToken cancellationToken = default(CancellationToken)) {
			return base.PrepareRequestUserAuthorizationAsync(callback, requestParameters, redirectParameters, cancellationToken);
		}

		/// <summary>
		/// Processes an incoming authorization-granted message from an SP and obtains an access token.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The access token, or null if no incoming authorization message was recognized.</returns>
		public async Task<AuthorizedTokenResponse> ProcessUserAuthorizationAsync(HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken)) {
			request = request ?? this.Channel.GetRequestFromContext();

			var authorizationMessage = await this.Channel.TryReadFromRequestAsync<UserAuthorizationResponse>(cancellationToken, request);
			if (authorizationMessage != null) {
				string requestToken = authorizationMessage.RequestToken;
				string verifier = authorizationMessage.VerificationCode;
				return await this.ProcessUserAuthorizationAsync(requestToken, verifier, cancellationToken);
			} else {
				return null;
			}
		}
	}
}
