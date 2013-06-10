//-----------------------------------------------------------------------
// <copyright file="WebServerClientAuthorizeTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class WebServerClientAuthorizeTests : OAuth2TestBase {
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

			var client = new WebServerClient(AuthorizationServerDescription, ClientId, ClientSecret, this.HostFactories);
			var authState = new AuthorizationState(TestScopes) {
				Callback = ClientCallback,
			};
			var authRequestRedirect = await client.PrepareRequestUserAuthorizationAsync(authState);
			this.HostFactories.CookieContainer.SetCookies(authRequestRedirect, ClientCallback);
			Uri authRequestResponse;
			this.HostFactories.AllowAutoRedirects = false;
			using (var httpClient = this.HostFactories.CreateHttpClient()) {
				using (var httpResponse = await httpClient.GetAsync(authRequestRedirect.Headers.Location)) {
					Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
					authRequestResponse = httpResponse.Headers.Location;
				}
			}

			var authorizationResponse = new HttpRequestMessage(HttpMethod.Get, authRequestResponse);
			this.HostFactories.CookieContainer.ApplyCookies(authorizationResponse);
			var result = await client.ProcessUserAuthorizationAsync(authorizationResponse, CancellationToken.None);
			Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
			Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
		}

		[Theory]
		public async Task ResourceOwnerPasswordCredentialGrant(bool anonymousClient) {
			var authHostMock = CreateAuthorizationServerMock();
			if (anonymousClient) {
				authHostMock.Setup(
					m =>
					m.IsAuthorizationValid(
						It.Is<IAuthorizationDescription>(
							d =>
							d.ClientIdentifier == null && d.User == ResourceOwnerUsername &&
							MessagingUtilities.AreEquivalent(d.Scope, TestScopes)))).Returns(true);
			}

			Handle(AuthorizationServerDescription.TokenEndpoint).By(async (req, ct) => {
				var server = new AuthorizationServer(authHostMock.Object);
				return await server.HandleTokenRequestAsync(req, ct);
			});

			var client = new WebServerClient(AuthorizationServerDescription, ClientId, ClientSecret, this.HostFactories);
			if (anonymousClient) {
				client.ClientIdentifier = null;
			}

			var authState = await client.ExchangeUserCredentialForTokenAsync(ResourceOwnerUsername, ResourceOwnerPassword, TestScopes);
			Assert.That(authState.AccessToken, Is.Not.Null.And.Not.Empty);
			Assert.That(authState.RefreshToken, Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public async Task ClientCredentialGrant() {
			var authServer = CreateAuthorizationServerMock();
			authServer.Setup(
				a => a.IsAuthorizationValid(It.Is<IAuthorizationDescription>(d => d.User == null && d.ClientIdentifier == ClientId && MessagingUtilities.AreEquivalent(d.Scope, TestScopes))))
				.Returns(true);
			authServer.Setup(
				a => a.CheckAuthorizeClientCredentialsGrant(It.Is<IAccessTokenRequest>(d => d.ClientIdentifier == ClientId && MessagingUtilities.AreEquivalent(d.Scope, TestScopes))))
				.Returns<IAccessTokenRequest>(req => new AutomatedAuthorizationCheckResponse(req, true));
			Handle(AuthorizationServerDescription.TokenEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(authServer.Object);
					return await server.HandleTokenRequestAsync(req, ct);
				});
			var client = new WebServerClient(AuthorizationServerDescription, ClientId, ClientSecret, this.HostFactories);
			var authState = await client.GetClientAccessTokenAsync(TestScopes);
			Assert.That(authState.AccessToken, Is.Not.Null.And.Not.Empty);
			Assert.That(authState.RefreshToken, Is.Null);
		}

		[Test]
		public async Task GetClientAccessTokenReturnsApprovedScope() {
			string[] approvedScopes = new[] { "Scope2", "Scope3" };
			var authServer = CreateAuthorizationServerMock();
			authServer.Setup(
				a => a.IsAuthorizationValid(It.Is<IAuthorizationDescription>(d => d.User == null && d.ClientIdentifier == ClientId && MessagingUtilities.AreEquivalent(d.Scope, TestScopes))))
					  .Returns(true);
			authServer.Setup(
				a => a.CheckAuthorizeClientCredentialsGrant(It.Is<IAccessTokenRequest>(d => d.ClientIdentifier == ClientId && MessagingUtilities.AreEquivalent(d.Scope, TestScopes))))
					.Returns<IAccessTokenRequest>(req => {
						var response = new AutomatedAuthorizationCheckResponse(req, true);
						response.ApprovedScope.ResetContents(approvedScopes);
						return response;
					});
			Handle(AuthorizationServerDescription.TokenEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(authServer.Object);
					return await server.HandleTokenRequestAsync(req, ct);
				});

			var client = new WebServerClient(AuthorizationServerDescription, ClientId, ClientSecret, this.HostFactories);
			var authState = await client.GetClientAccessTokenAsync(TestScopes);
			Assert.That(authState.Scope, Is.EquivalentTo(approvedScopes));
		}

		[Test]
		public void CreateAuthorizingHandlerBearer() {
			var client = new WebServerClient(AuthorizationServerDescription);
			string bearerToken = "mytoken";
			var tcs = new TaskCompletionSource<HttpResponseMessage>();
			var expectedResponse = new HttpResponseMessage();

			var mockHandler = new DotNetOpenAuth.Test.Mocks.MockHttpMessageHandler((req, ct) => {
				Assert.That(req.Headers.Authorization.Scheme, Is.EqualTo(Protocol.BearerHttpAuthorizationScheme));
				Assert.That(req.Headers.Authorization.Parameter, Is.EqualTo(bearerToken));
				tcs.SetResult(expectedResponse);
				return tcs.Task;
			});
			var applicator = client.CreateAuthorizingHandler("mytoken", mockHandler);
			var httpClient = new HttpClient(applicator);
			var actualResponse = httpClient.GetAsync("http://localhost/someMessage").Result;
			Assert.That(actualResponse, Is.SameAs(expectedResponse));
		}

		[Test]
		public void CreateAuthorizingHandlerAuthorization() {
			var client = new WebServerClient(AuthorizationServerDescription);
			string bearerToken = "mytoken";
			var authorization = new Mock<IAuthorizationState>();
			authorization.SetupGet(a => a.AccessToken).Returns(bearerToken);
			var tcs = new TaskCompletionSource<HttpResponseMessage>();
			var expectedResponse = new HttpResponseMessage();

			var mockHandler = new DotNetOpenAuth.Test.Mocks.MockHttpMessageHandler((req, ct) => {
				Assert.That(req.Headers.Authorization.Scheme, Is.EqualTo(Protocol.BearerHttpAuthorizationScheme));
				Assert.That(req.Headers.Authorization.Parameter, Is.EqualTo(bearerToken));
				tcs.SetResult(expectedResponse);
				return tcs.Task;
			});
			var applicator = client.CreateAuthorizingHandler(authorization.Object, mockHandler);
			var httpClient = new HttpClient(applicator);
			var actualResponse = httpClient.GetAsync("http://localhost/someMessage").Result;
			Assert.That(actualResponse, Is.SameAs(expectedResponse));
		}
	}
}
