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
					Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
					Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
				},
				server => {
					var request = server.ReadAuthorizationRequest();
					Assert.That(request, Is.Not.Null);
					server.ApproveAuthorizationRequest(request, ResourceOwnerUsername);
					server.HandleTokenRequest().Respond();
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
					Assert.That(request.ResponseType, Is.EqualTo(EndUserAuthorizationResponseType.AccessToken));
					client.Channel.Respond(request);
					var incoming = client.Channel.ReadFromRequest();
					var result = client.ProcessUserAuthorization(authState, incoming);
					Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
					Assert.That(result.RefreshToken, Is.Null);
				},
				server => {
					var request = server.ReadAuthorizationRequest();
					Assert.That(request, Is.Not.Null);
					IAccessTokenRequest accessTokenRequest = (EndUserAuthorizationImplicitRequest)request;
					Assert.That(accessTokenRequest.ClientAuthenticated, Is.False);
					server.ApproveAuthorizationRequest(request, ResourceOwnerUsername);
				});

			coordinatorClient.ClientCredentialApplicator = null; // implicit grant clients don't need a secret.
			coordinator.Run();
		}
	}
}
