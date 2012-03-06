//-----------------------------------------------------------------------
// <copyright file="WebConsumer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

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
		public UserAuthorizationRequest PrepareRequestUserAuthorization() {
			Uri callback = this.Channel.GetRequestFromContext().GetPublicFacingUrl().StripQueryArgumentsWithPrefix(Protocol.ParameterPrefix);
			return this.PrepareRequestUserAuthorization(callback, null, null);
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
		public UserAuthorizationRequest PrepareRequestUserAuthorization(Uri callback, IDictionary<string, string> requestParameters, IDictionary<string, string> redirectParameters) {
			string token;
			return this.PrepareRequestUserAuthorization(callback, requestParameters, redirectParameters, out token);
		}

		/// <summary>
		/// Processes an incoming authorization-granted message from an SP and obtains an access token.
		/// </summary>
		/// <returns>The access token, or null if no incoming authorization message was recognized.</returns>
		/// <remarks>
		/// Requires HttpContext.Current.
		/// </remarks>
		public AuthorizedTokenResponse ProcessUserAuthorization() {
			return this.ProcessUserAuthorization(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Processes an incoming authorization-granted message from an SP and obtains an access token.
		/// </summary>
		/// <param name="request">The incoming HTTP request.</param>
		/// <returns>The access token, or null if no incoming authorization message was recognized.</returns>
		public AuthorizedTokenResponse ProcessUserAuthorization(HttpRequestBase request) {
			Requires.NotNull(request, "request");

			UserAuthorizationResponse authorizationMessage;
			if (this.Channel.TryReadFromRequest<UserAuthorizationResponse>(request, out authorizationMessage)) {
				string requestToken = authorizationMessage.RequestToken;
				string verifier = authorizationMessage.VerificationCode;
				return this.ProcessUserAuthorization(requestToken, verifier);
			} else {
				return null;
			}
		}
	}
}
