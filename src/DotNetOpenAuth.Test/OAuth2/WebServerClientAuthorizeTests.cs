//-----------------------------------------------------------------------
// <copyright file="WebServerClientAuthorizeTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.Messages;
	using NUnit.Framework;

	[TestFixture]
	public class WebServerClientAuthorizeTests : OAuth2TestBase {
		[TestCase]
		public void AuthorizationCodeGrant() {
			var coordinator = new OAuth2Coordinator<WebServerClient>(
				AuthorizationServerDescription,
				AuthorizationServerMock,
				new WebServerClient(AuthorizationServerDescription),
				client => {
					var authState = new AuthorizationState {
						Callback = ClientCallback,
					};
					client.PrepareRequestUserAuthorization(authState).Respond();
					var result = client.ProcessUserAuthorization();
					Assert.IsNotNullOrEmpty(result.AccessToken);
					Assert.IsNotNullOrEmpty(result.RefreshToken);
				},
				server => {
					var request = server.ReadAuthorizationRequest();
					server.ApproveAuthorizationRequest(request, ResourceOwnerUsername);
					var tokenRequest = server.ReadAccessTokenRequest();
					IAccessTokenRequest accessTokenRequest = tokenRequest;
					Assert.IsTrue(accessTokenRequest.ClientAuthenticated);
					var tokenResponse = server.PrepareAccessTokenResponse(tokenRequest);
					server.Channel.Respond(tokenResponse);
				});
			coordinator.Run();
		}

		[TestCase]
		public void ResourceOwnerPasswordCredentialGrant() {
			var coordinator = new OAuth2Coordinator<WebServerClient>(
				AuthorizationServerDescription,
				AuthorizationServerMock,
				new WebServerClient(AuthorizationServerDescription),
				client => {
					var authState = client.ExchangeUserCredentialForToken(ResourceOwnerUsername, ResourceOwnerPassword);
					Assert.IsNotNullOrEmpty(authState.AccessToken);
					Assert.IsNotNullOrEmpty(authState.RefreshToken);
				},
				server => {
					var request = server.ReadAccessTokenRequest();
					var response = server.PrepareAccessTokenResponse(request);
					server.Channel.Respond(response);
				});
			coordinator.Run();
		}
	}
}
