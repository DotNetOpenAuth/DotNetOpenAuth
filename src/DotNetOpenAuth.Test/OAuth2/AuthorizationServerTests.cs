//-----------------------------------------------------------------------
// <copyright file="AuthorizationServerTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;
	using Moq;
	using NUnit.Framework;

	/// <summary>
	/// Verifies authorization server functionality.
	/// </summary>
	[TestFixture]
	public class AuthorizationServerTests : OAuth2TestBase {
		/// <summary>
		/// Verifies that authorization server responds with an appropriate error response.
		/// </summary>
		[Test]
		public async Task ErrorResponseTest() {
			Handle(AuthorizationServerDescription.TokenEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(AuthorizationServerMock);
					return await server.HandleTokenRequestAsync(req, ct);
				});
			var request = new AccessTokenAuthorizationCodeRequestC(AuthorizationServerDescription) { ClientIdentifier = ClientId, ClientSecret = ClientSecret, AuthorizationCode = "foo" };
			var client = new UserAgentClient(AuthorizationServerDescription, hostFactories: this.HostFactories);
			var response = await client.Channel.RequestAsync<AccessTokenFailedResponse>(request, CancellationToken.None);
			Assert.That(response.Error, Is.Not.Null.And.Not.Empty);
			Assert.That(response.Error, Is.EqualTo(Protocol.AccessTokenRequestErrorCodes.InvalidRequest));
		}

		[Test]
		public async Task DecodeRefreshToken() {
			var refreshTokenSource = new TaskCompletionSource<string>();
			Handle(AuthorizationServerDescription.AuthorizationEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(AuthorizationServerMock);
					var request = await server.ReadAuthorizationRequestAsync(req, ct);
					Assert.That(request, Is.Not.Null);
					var response = server.PrepareApproveAuthorizationRequest(request, ResourceOwnerUsername);
					return await server.Channel.PrepareResponseAsync(response);
				});
			Handle(AuthorizationServerDescription.TokenEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(AuthorizationServerMock);
					var response = await server.HandleTokenRequestAsync(req, ct);
					var authorization = server.DecodeRefreshToken(refreshTokenSource.Task.Result);
					Assert.That(authorization, Is.Not.Null);
					Assert.That(authorization.User, Is.EqualTo(ResourceOwnerUsername));
					return response;
				});

			var client = new WebServerClient(AuthorizationServerDescription);
			try {
				var authState = new AuthorizationState(TestScopes) { Callback = ClientCallback, };
				var authRedirectResponse = await client.PrepareRequestUserAuthorizationAsync(authState);
				this.HostFactories.CookieContainer.SetCookies(authRedirectResponse, ClientCallback);
				Uri authCompleteUri;
				using (var httpClient = this.HostFactories.CreateHttpClient()) {
					using (var response = await httpClient.GetAsync(authRedirectResponse.Headers.Location)) {
						response.EnsureSuccessStatusCode();
						authCompleteUri = response.Headers.Location;
					}
				}

				var authCompleteRequest = new HttpRequestMessage(HttpMethod.Get, authCompleteUri);
				this.HostFactories.CookieContainer.ApplyCookies(authCompleteRequest);
				var result = await client.ProcessUserAuthorizationAsync(authCompleteRequest);
				Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
				Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
				refreshTokenSource.SetResult(result.RefreshToken);
			} catch {
				refreshTokenSource.TrySetCanceled();
			}
		}

		[Test]
		public async Task ResourceOwnerScopeOverride() {
			var clientRequestedScopes = new[] { "scope1", "scope2" };
			var serverOverriddenScopes = new[] { "scope1", "differentScope" };
			var authServerMock = CreateAuthorizationServerMock();
			authServerMock
				.Setup(a => a.CheckAuthorizeResourceOwnerCredentialGrant(ResourceOwnerUsername, ResourceOwnerPassword, It.IsAny<IAccessTokenRequest>()))
				.Returns<string, string, IAccessTokenRequest>((un, pw, req) => {
					var response = new AutomatedUserAuthorizationCheckResponse(req, true, ResourceOwnerUsername);
					response.ApprovedScope.Clear();
					response.ApprovedScope.UnionWith(serverOverriddenScopes);
					return response;
				});

			Handle(AuthorizationServerDescription.TokenEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(authServerMock.Object);
					return await server.HandleTokenRequestAsync(req, ct);
				});

			var client = new WebServerClient(AuthorizationServerDescription, hostFactories: this.HostFactories);
			var result = await client.ExchangeUserCredentialForTokenAsync(ResourceOwnerUsername, ResourceOwnerPassword, clientRequestedScopes);
			Assert.That(result.Scope, Is.EquivalentTo(serverOverriddenScopes));
		}

		[Test]
		public async Task CreateAccessTokenSeesAuthorizingUserResourceOwnerGrant() {
			var authServerMock = CreateAuthorizationServerMock();
			authServerMock
				.Setup(a => a.CheckAuthorizeResourceOwnerCredentialGrant(ResourceOwnerUsername, ResourceOwnerPassword, It.IsAny<IAccessTokenRequest>()))
				.Returns<string, string, IAccessTokenRequest>((un, pw, req) => {
					var response = new AutomatedUserAuthorizationCheckResponse(req, true, ResourceOwnerUsername);
					Assert.That(req.UserName, Is.EqualTo(ResourceOwnerUsername));
					return response;
				});

			Handle(AuthorizationServerDescription.TokenEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(authServerMock.Object);
					return await server.HandleTokenRequestAsync(req, ct);
				});

			var client = new WebServerClient(AuthorizationServerDescription, hostFactories: this.HostFactories);
			var result = await client.ExchangeUserCredentialForTokenAsync(ResourceOwnerUsername, ResourceOwnerPassword, TestScopes);
			Assert.That(result.AccessToken, Is.Not.Null);
		}

		[Test]
		public async Task CreateAccessTokenSeesAuthorizingUserClientCredentialGrant() {
			var authServerMock = CreateAuthorizationServerMock();
			authServerMock
				.Setup(a => a.CheckAuthorizeClientCredentialsGrant(It.IsAny<IAccessTokenRequest>()))
				.Returns<IAccessTokenRequest>(req => {
					Assert.That(req.UserName, Is.Null);
					return new AutomatedAuthorizationCheckResponse(req, true);
				});

			Handle(AuthorizationServerDescription.TokenEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(authServerMock.Object);
					return await server.HandleTokenRequestAsync(req, ct);
				});

			var client = new WebServerClient(AuthorizationServerDescription, ClientId, ClientSecret, this.HostFactories);
			var result = await client.GetClientAccessTokenAsync(TestScopes);
			Assert.That(result.AccessToken, Is.Not.Null);
		}

		[Test]
		public async Task CreateAccessTokenSeesAuthorizingUserAuthorizationCodeGrant() {
			var authServerMock = CreateAuthorizationServerMock();
			authServerMock
				.Setup(a => a.IsAuthorizationValid(It.IsAny<IAuthorizationDescription>()))
				.Returns<IAuthorizationDescription>(req => {
					Assert.That(req.User, Is.EqualTo(ResourceOwnerUsername));
					return true;
				});

			Handle(AuthorizationServerDescription.AuthorizationEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(authServerMock.Object);
					var request = await server.ReadAuthorizationRequestAsync(req, ct);
					Assert.That(request, Is.Not.Null);
					var response = server.PrepareApproveAuthorizationRequest(request, ResourceOwnerUsername);
					return await server.Channel.PrepareResponseAsync(response);
				});
			Handle(AuthorizationServerDescription.TokenEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(authServerMock.Object);
					return await server.HandleTokenRequestAsync(req, ct);
				});

			var client = new WebServerClient(AuthorizationServerDescription, ClientId, ClientSecret, this.HostFactories);
			var authState = new AuthorizationState(TestScopes) {
				Callback = ClientCallback,
			};
			var authRedirectResponse = await client.PrepareRequestUserAuthorizationAsync(authState);
			this.HostFactories.CookieContainer.SetCookies(authRedirectResponse, ClientCallback);
			Uri authCompleteUri;
			this.HostFactories.AllowAutoRedirects = false;
			using (var httpClient = this.HostFactories.CreateHttpClient()) {
				using (var response = await httpClient.GetAsync(authRedirectResponse.Headers.Location)) {
					Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
					authCompleteUri = response.Headers.Location;
				}
			}

			var authCompleteRequest = new HttpRequestMessage(HttpMethod.Get, authCompleteUri);
			this.HostFactories.CookieContainer.ApplyCookies(authCompleteRequest);
			var result = await client.ProcessUserAuthorizationAsync(authCompleteRequest);
			Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
			Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public async Task ClientCredentialScopeOverride() {
			var clientRequestedScopes = new[] { "scope1", "scope2" };
			var serverOverriddenScopes = new[] { "scope1", "differentScope" };
			var authServerMock = CreateAuthorizationServerMock();
			authServerMock
				.Setup(a => a.CheckAuthorizeClientCredentialsGrant(It.IsAny<IAccessTokenRequest>()))
				.Returns<IAccessTokenRequest>(req => {
					var response = new AutomatedAuthorizationCheckResponse(req, true);
					response.ApprovedScope.Clear();
					response.ApprovedScope.UnionWith(serverOverriddenScopes);
					return response;
				});

			Handle(AuthorizationServerDescription.TokenEndpoint).By(
				async (req, ct) => {
					var server = new AuthorizationServer(authServerMock.Object);
					return await server.HandleTokenRequestAsync(req, ct);
				});

			var client = new WebServerClient(AuthorizationServerDescription, ClientId, ClientSecret, this.HostFactories);
			var result = await client.GetClientAccessTokenAsync(clientRequestedScopes);
			Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
			Assert.That(result.Scope, Is.EquivalentTo(serverOverriddenScopes));
		}
	}
}
