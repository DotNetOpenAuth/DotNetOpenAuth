//-----------------------------------------------------------------------
// <copyright file="AuthorizationServerTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using DotNetOpenAuth.OAuth2;
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
		public void ErrorResponseTest() {
			var coordinator = new OAuth2Coordinator<UserAgentClient>(
				AuthorizationServerDescription,
				AuthorizationServerMock,
				new UserAgentClient(AuthorizationServerDescription),
				client => {
					var request = new AccessTokenAuthorizationCodeRequestC(AuthorizationServerDescription) { ClientIdentifier = ClientId, ClientSecret = ClientSecret, AuthorizationCode = "foo" };

					var response = client.Channel.Request<AccessTokenFailedResponse>(request);
					Assert.That(response.Error, Is.Not.Null.And.Not.Empty);
					Assert.That(response.Error, Is.EqualTo(Protocol.AccessTokenRequestErrorCodes.InvalidRequest));
				},
				server => {
					server.HandleTokenRequest().Respond();
				});
			coordinator.Run();
		}

		[Test]
		public void DecodeRefreshToken() {
			var refreshTokenSource = new TaskCompletionSource<string>();
			var coordinator = new OAuth2Coordinator<WebServerClient>(
				AuthorizationServerDescription,
				AuthorizationServerMock,
				new WebServerClient(AuthorizationServerDescription),
				client => {
					try {
						var authState = new AuthorizationState(TestScopes) {
							Callback = ClientCallback,
						};
						client.PrepareRequestUserAuthorization(authState).Respond();
						var result = client.ProcessUserAuthorization();
						Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
						Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
						refreshTokenSource.SetResult(result.RefreshToken);
					} catch {
						refreshTokenSource.TrySetCanceled();
					}
				},
				server => {
					var request = server.ReadAuthorizationRequest();
					Assert.That(request, Is.Not.Null);
					server.ApproveAuthorizationRequest(request, ResourceOwnerUsername);
					server.HandleTokenRequest().Respond();
					var authorization = server.DecodeRefreshToken(refreshTokenSource.Task.Result);
					Assert.That(authorization, Is.Not.Null);
					Assert.That(authorization.User, Is.EqualTo(ResourceOwnerUsername));
				});
			coordinator.Run();
		}

		[Test]
		public void ResourceOwnerScopeOverride() {
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
			var coordinator = new OAuth2Coordinator<WebServerClient>(
				AuthorizationServerDescription,
				authServerMock.Object,
				new WebServerClient(AuthorizationServerDescription),
				client => {
					var authState = new AuthorizationState(TestScopes) {
						Callback = ClientCallback,
					};
					var result = client.ExchangeUserCredentialForToken(ResourceOwnerUsername, ResourceOwnerPassword, clientRequestedScopes);
					Assert.That(result.Scope, Is.EquivalentTo(serverOverriddenScopes));
				},
				server => {
					server.HandleTokenRequest().Respond();
				});
			coordinator.Run();
		}

		[Test]
		public void ClientCredentialScopeOverride() {
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
			var coordinator = new OAuth2Coordinator<WebServerClient>(
				AuthorizationServerDescription,
				authServerMock.Object,
				new WebServerClient(AuthorizationServerDescription),
				client => {
					var authState = new AuthorizationState(TestScopes) {
						Callback = ClientCallback,
					};
					var result = client.GetClientAccessToken(clientRequestedScopes);
					Assert.That(result.Scope, Is.EquivalentTo(serverOverriddenScopes));
				},
				server => {
					server.HandleTokenRequest().Respond();
				});
			coordinator.Run();
		}
	}
}
