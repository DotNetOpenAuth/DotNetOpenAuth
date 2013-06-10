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
	using System.Threading;
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
			Handle(AuthorizationServerDescription.AuthorizationEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(AuthorizationServerMock);
					var request = await server.ReadAuthorizationRequestAsync(req, ct);
					Assert.That(request, Is.Not.Null);
					var response = server.PrepareApproveAuthorizationRequest(request, ResourceOwnerUsername);
					return await server.Channel.PrepareResponseAsync(response, ct);
				});
			Handle(AuthorizationServerDescription.TokenEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(AuthorizationServerMock);
					return await server.HandleTokenRequestAsync(req, ct);
				});
			{
				var client = new UserAgentClient(AuthorizationServerDescription, ClientId, ClientSecret, this.HostFactories);
				var authState = new AuthorizationState(TestScopes) { Callback = ClientCallback, };
				var request = client.PrepareRequestUserAuthorization(authState);
				Assert.AreEqual(EndUserAuthorizationResponseType.AuthorizationCode, request.ResponseType);
				var authRequestRedirect = await client.Channel.PrepareResponseAsync(request);
				Uri authRequestResponse;
				this.HostFactories.AllowAutoRedirects = false;
				using (var httpClient = this.HostFactories.CreateHttpClient()) {
					using (var httpResponse = await httpClient.GetAsync(authRequestRedirect.Headers.Location)) {
						authRequestResponse = httpResponse.Headers.Location;
					}
				}
				var incoming =
					await
					client.Channel.ReadFromRequestAsync(
						new HttpRequestMessage(HttpMethod.Get, authRequestResponse), CancellationToken.None);
				var result = await client.ProcessUserAuthorizationAsync(authState, incoming, CancellationToken.None);
				Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
				Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
			}
		}

		[Test]
		public async Task ImplicitGrant() {
			var coordinatorClient = new UserAgentClient(AuthorizationServerDescription);
			coordinatorClient.ClientCredentialApplicator = null; // implicit grant clients don't need a secret.
			Handle(AuthorizationServerDescription.AuthorizationEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(AuthorizationServerMock);
					var request = await server.ReadAuthorizationRequestAsync(req, ct);
					Assert.That(request, Is.Not.Null);
					IAccessTokenRequest accessTokenRequest = (EndUserAuthorizationImplicitRequest)request;
					Assert.That(accessTokenRequest.ClientAuthenticated, Is.False);
					var response = server.PrepareApproveAuthorizationRequest(request, ResourceOwnerUsername);
					return await server.Channel.PrepareResponseAsync(response, ct);
				});
			{
				var client = new UserAgentClient(AuthorizationServerDescription, ClientId, ClientSecret, this.HostFactories);
				var authState = new AuthorizationState(TestScopes) { Callback = ClientCallback, };
				var request = client.PrepareRequestUserAuthorization(authState, implicitResponseType: true);
				Assert.That(request.ResponseType, Is.EqualTo(EndUserAuthorizationResponseType.AccessToken));
				var authRequestRedirect = await client.Channel.PrepareResponseAsync(request);
				Uri authRequestResponse;
				this.HostFactories.AllowAutoRedirects = false;
				using (var httpClient = this.HostFactories.CreateHttpClient()) {
					using (var httpResponse = await httpClient.GetAsync(authRequestRedirect.Headers.Location)) {
						authRequestResponse = httpResponse.Headers.Location;
					}
				}

				var incoming =
					await client.Channel.ReadFromRequestAsync(new HttpRequestMessage(HttpMethod.Get, authRequestResponse), CancellationToken.None);
				var result = await client.ProcessUserAuthorizationAsync(authState, incoming, CancellationToken.None);
				Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
				Assert.That(result.RefreshToken, Is.Null);
			}
		}
	}
}
