//-----------------------------------------------------------------------
// <copyright file="WebConsumerOpenIdRelyingParty.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.OpenId.Extensions.OAuth;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Validation;

	/// <summary>
	/// A website or application that uses OAuth to access the Service Provider on behalf of the User
	/// and can attach OAuth requests to outbound OpenID authentication requests.
	/// </summary>
	/// <remarks>
	/// The methods on this class are thread-safe.  Provided the properties are set and not changed
	/// afterward, a single instance of this class may be used by an entire web application safely.
	/// </remarks>
	public class WebConsumerOpenIdRelyingParty : Consumer {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebConsumerOpenIdRelyingParty"/> class.
		/// </summary>
		public WebConsumerOpenIdRelyingParty() {
		}

		/// <summary>
		/// Attaches an OAuth authorization request to an outgoing OpenID authentication request.
		/// </summary>
		/// <param name="openIdAuthenticationRequest">The OpenID authentication request.</param>
		/// <param name="scope">The scope of access that is requested of the service provider.</param>
		public void AttachAuthorizationRequest(IAuthenticationRequest openIdAuthenticationRequest, string scope) {
			Requires.NotNull(openIdAuthenticationRequest, "openIdAuthenticationRequest");

			var authorizationRequest = new AuthorizationRequest {
				Consumer = this.ConsumerKey,
				Scope = scope,
			};

			openIdAuthenticationRequest.AddExtension(authorizationRequest);
		}

		/// <summary>
		/// Processes an incoming authorization-granted message from an SP and obtains an access token.
		/// </summary>
		/// <param name="openIdAuthenticationResponse">The OpenID authentication response that may be carrying an authorized request token.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The access token, or null if OAuth authorization was denied by the user or service provider.
		/// </returns>
		public async Task<AccessTokenResponse> ProcessUserAuthorizationAsync(IAuthenticationResponse openIdAuthenticationResponse, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(openIdAuthenticationResponse, "openIdAuthenticationResponse");

			// The OAuth extension is only expected in positive assertion responses.
			if (openIdAuthenticationResponse.Status != AuthenticationStatus.Authenticated) {
				return null;
			}

			// Retrieve the OAuth extension
			var positiveAuthorization = openIdAuthenticationResponse.GetExtension<AuthorizationApprovedResponse>();
			if (positiveAuthorization == null) {
				return null;
			}

			using (var client = this.CreateHttpClient(new AccessToken(positiveAuthorization.RequestToken, string.Empty))) {
				var request = new HttpRequestMessage(this.ServiceProvider.TokenRequestEndpointMethod, this.ServiceProvider.TokenRequestEndpoint);
				using (var response = await client.SendAsync(request, cancellationToken)) {
					response.EnsureSuccessStatusCode();

					// Parse the response and ensure that it meets the requirements of the OAuth 1.0 spec.
					string content = await response.Content.ReadAsStringAsync();
					var responseData = HttpUtility.ParseQueryString(content);
					string accessToken = responseData[Protocol.TokenParameter];
					string tokenSecret = responseData[Protocol.TokenSecretParameter];
					ErrorUtilities.VerifyProtocol(!string.IsNullOrEmpty(accessToken), MessagingStrings.RequiredParametersMissing, typeof(AuthorizedTokenResponse).Name, Protocol.TokenParameter);
					ErrorUtilities.VerifyProtocol(tokenSecret != null, MessagingStrings.RequiredParametersMissing, typeof(AuthorizedTokenResponse).Name, Protocol.TokenSecretParameter);

					responseData.Remove(Protocol.TokenParameter);
					responseData.Remove(Protocol.TokenSecretParameter);
					return new AccessTokenResponse(accessToken, tokenSecret, responseData);
				}
			}
		}
	}
}
