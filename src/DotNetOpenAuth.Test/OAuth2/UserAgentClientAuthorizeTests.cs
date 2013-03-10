//-----------------------------------------------------------------------
// <copyright file="UserAgentClientAuthorizeTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;
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
		public async Task AuthorizationCodeGrant() {
			var coordinator = new CoordinatorBase(
				async (hostFactories, ct) => {
					var client = new UserAgentClient(AuthorizationServerDescription);
					var authState = new AuthorizationState(TestScopes) {
						Callback = ClientCallback,
					};
					var request = client.PrepareRequestUserAuthorization(authState);
					Assert.AreEqual(EndUserAuthorizationResponseType.AuthorizationCode, request.ResponseType);
					var authRequestRedirect = await client.Channel.PrepareResponseAsync(request, ct);
					Uri authRequestResponse;
					using (var httpClient = hostFactories.CreateHttpClient()) {
						using (var httpResponse = await httpClient.GetAsync(authRequestRedirect.Headers.Location, ct)) {
							authRequestResponse = httpResponse.Headers.Location;
						}
					}
					var incoming = await client.Channel.ReadFromRequestAsync(new HttpRequestMessage(HttpMethod.Get, authRequestResponse), ct);
					var result = await client.ProcessUserAuthorizationAsync(authState, incoming, ct);
					Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
					Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
				},
				CoordinatorBase.Handle(AuthorizationServerDescription.AuthorizationEndpoint).By(
					async (req, ct) => {
						var server = new AuthorizationServer(AuthorizationServerMock);
						var request = await server.ReadAuthorizationRequestAsync(req, ct);
						Assert.That(request, Is.Not.Null);
						var response = server.PrepareApproveAuthorizationRequest(request, ResourceOwnerUsername);
						return await server.Channel.PrepareResponseAsync(response, ct);
					}),
					CoordinatorBase.Handle(AuthorizationServerDescription.TokenEndpoint).By(
						async (req, ct) => {
							var server = new AuthorizationServer(AuthorizationServerMock);
							return await server.HandleTokenRequestAsync(req, ct);
						}));
			await coordinator.RunAsync();
		}

		[Test]
		public async Task ImplicitGrant() {
			var coordinatorClient = new UserAgentClient(AuthorizationServerDescription);
			var coordinator = new CoordinatorBase(
				async (hostFactories, ct) => {
					var client = new UserAgentClient(AuthorizationServerDescription);
					var authState = new AuthorizationState(TestScopes) {
						Callback = ClientCallback,
					};
					var request = client.PrepareRequestUserAuthorization(authState, implicitResponseType: true);
					Assert.That(request.ResponseType, Is.EqualTo(EndUserAuthorizationResponseType.AccessToken));
					var authRequestRedirect = await client.Channel.PrepareResponseAsync(request, ct);
					Uri authRequestResponse;
					using (var httpClient = hostFactories.CreateHttpClient()) {
						using (var httpResponse = await httpClient.GetAsync(authRequestRedirect.Headers.Location, ct)) {
							authRequestResponse = httpResponse.Headers.Location;
						}
					}

					var incoming = await client.Channel.ReadFromRequestAsync(new HttpRequestMessage(HttpMethod.Get, authRequestResponse), ct);
					var result = await client.ProcessUserAuthorizationAsync(authState, incoming, ct);
					Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
					Assert.That(result.RefreshToken, Is.Null);
				},
				CoordinatorBase.Handle(AuthorizationServerDescription.AuthorizationEndpoint).By(
					async (req, ct) => {
						var server = new AuthorizationServer(AuthorizationServerMock);
						var request = await server.ReadAuthorizationRequestAsync(req, ct);
						Assert.That(request, Is.Not.Null);
						IAccessTokenRequest accessTokenRequest = (EndUserAuthorizationImplicitRequest)request;
						Assert.That(accessTokenRequest.ClientAuthenticated, Is.False);
						var response = server.PrepareApproveAuthorizationRequest(request, ResourceOwnerUsername);
						return await server.Channel.PrepareResponseAsync(response, ct);
					}));

			coordinatorClient.ClientCredentialApplicator = null; // implicit grant clients don't need a secret.
			await coordinator.RunAsync();
		}
	}
}
