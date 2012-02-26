//-----------------------------------------------------------------------
// <copyright file="UserAgentClientAuthorizeTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class UserAgentClientAuthorizeTests : OAuth2TestBase {
		[Test]
		public void AuthorizationCodeGrant() {
			var coordinator = new OAuth2Coordinator<UserAgentClient>(
				AuthorizationServerDescription,
				AuthorizationServerMock,
				new UserAgentClient(AuthorizationServerDescription),
				client => {
					var authState = new AuthorizationState(TestScopes) {
						Callback = ClientCallback,
					};
					var request = client.PrepareRequestUserAuthorization(authState);
					Assert.AreEqual(EndUserAuthorizationResponseType.AuthorizationCode, request.ResponseType);
					client.Channel.Respond(request);
					var incoming = client.Channel.ReadFromRequest();
					var result = client.ProcessUserAuthorization(authState, incoming);
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

		[Test]
		public void ImplicitGrant() {
			var coordinatorClient = new UserAgentClient(AuthorizationServerDescription);
			var coordinator = new OAuth2Coordinator<UserAgentClient>(
				AuthorizationServerDescription,
				AuthorizationServerMock,
				coordinatorClient,
				client => {
					var authState = new AuthorizationState(TestScopes) {
						Callback = ClientCallback,
					};
					var request = client.PrepareRequestUserAuthorization(authState, implicitResponseType: true);
					Assert.AreEqual(EndUserAuthorizationResponseType.AccessToken, request.ResponseType);
					client.Channel.Respond(request);
					var incoming = client.Channel.ReadFromRequest();
					var result = client.ProcessUserAuthorization(authState, incoming);
					Assert.IsNotNullOrEmpty(result.AccessToken);
					Assert.IsNull(result.RefreshToken);
				},
				server => {
					var request = server.ReadAuthorizationRequest();
					IAccessTokenRequest accessTokenRequest = (EndUserAuthorizationImplicitRequest)request;
					Assert.IsFalse(accessTokenRequest.ClientAuthenticated);
					server.ApproveAuthorizationRequest(request, ResourceOwnerUsername);
				});

			coordinatorClient.ClientSecret = null; // implicit grant clients don't need a secret.
			coordinator.Run();
		}
	}
}
