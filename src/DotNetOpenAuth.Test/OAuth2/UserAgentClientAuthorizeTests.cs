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
		[TestCase]
		public void AuthorizationCodeGrantAuthorization() {
			var coordinator = new OAuth2Coordinator<UserAgentClient>(
				AuthorizationServerDescription,
				AuthorizationServerMock,
				new UserAgentClient(AuthorizationServerDescription),
				client => {
					var authState = new AuthorizationState {
						Callback = ClientCallback,
					};
					var request = client.PrepareRequestUserAuthorization(authState);
					client.Channel.Respond(request);
					var incoming = client.Channel.ReadFromRequest();
					var result = client.ProcessUserAuthorization(authState, incoming);
					Assert.IsNotNullOrEmpty(result.AccessToken);
					Assert.IsNotNullOrEmpty(result.RefreshToken);
				},
				server => {
					var request = server.ReadAuthorizationRequest();
					server.ApproveAuthorizationRequest(request, Username);
					var tokenRequest = server.ReadAccessTokenRequest();
					IAccessTokenRequest accessTokenRequest = tokenRequest;
					Assert.IsTrue(accessTokenRequest.ClientAuthenticated);
					var tokenResponse = server.PrepareAccessTokenResponse(tokenRequest);
					server.Channel.Respond(tokenResponse);
				});
			coordinator.Run();
		}

		[TestCase]
		public void ImplicitGrantAuthorization() {
			var coordinatorClient = new UserAgentClient(AuthorizationServerDescription);
			var coordinator = new OAuth2Coordinator<UserAgentClient>(
				AuthorizationServerDescription,
				AuthorizationServerMock,
				coordinatorClient,
				client => {
					var authState = new AuthorizationState {
						Callback = ClientCallback,
					};
					var request = client.PrepareRequestUserAuthorization(authState);
					request.ResponseType = EndUserAuthorizationResponseType.AccessToken;
					client.Channel.Respond(request);
					var incoming = client.Channel.ReadFromRequest();
					var result = client.ProcessUserAuthorization(authState, incoming);
					Assert.IsNotNullOrEmpty(result.AccessToken);
					Assert.IsNull(result.RefreshToken);
				},
				server => {
					var request = server.ReadAuthorizationRequest();
					IAccessTokenRequest accessTokenRequest = request;
					Assert.IsFalse(accessTokenRequest.ClientAuthenticated);
					server.ApproveAuthorizationRequest(request, Username);
				});

			coordinatorClient.ClientSecret = null; // implicit grant clients don't need a secret.
			coordinator.Run();
		}
	}
}
