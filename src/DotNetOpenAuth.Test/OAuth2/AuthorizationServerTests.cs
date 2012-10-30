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
	}
}
