//-----------------------------------------------------------------------
// <copyright file="OAuthClientTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Test {
	using System;
	using System.Collections.Specialized;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.AspNet;
	using DotNetOpenAuth.AspNet.Clients;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.Messages;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class OAuthClientTest {
		[TestCase]
		public void TestProviderNamePropertyIsCorrect() {
			// Arrange
			var client = new MockOAuthClient();

			// Act
			var provider = client.ProviderName;

			// Assert
			Assert.AreEqual("mockoauth", provider);
		}

		[TestCase]
		public async Task RequestAuthenticationInvokeMethodOnWebWorker() {
			// Arrange
			var returnUri = new Uri("http://live.com/my/path.cshtml?q=one");
			var webWorker = new Mock<IOAuthWebWorker>(MockBehavior.Strict);
			webWorker
				.Setup(w => w.RequestAuthenticationAsync(returnUri, It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new Uri("http://someauth/uri")))
				.Verifiable();

			var client = new MockOAuthClient(webWorker.Object);
			var context = new Mock<HttpContextBase>();

			// Act
			await client.RequestAuthenticationAsync(context.Object, returnUri);

			// Assert
			webWorker.Verify();
		}

		[TestCase]
		public async Task VerifyAuthenticationFailsIfResponseTokenIsNull() {
			// Arrange
			var webWorker = new Mock<IOAuthWebWorker>(MockBehavior.Strict);
			webWorker.Setup(w => w.ProcessUserAuthorizationAsync(It.IsAny<HttpContextBase>(), CancellationToken.None)).Returns(Task.FromResult<AccessTokenResponse>(null));

			var client = new MockOAuthClient(webWorker.Object);
			var context = new Mock<HttpContextBase>();

			// Act
			await client.VerifyAuthenticationAsync(context.Object);

			// Assert
			webWorker.Verify();
		}

		[TestCase]
		public async Task VerifyAuthenticationFailsIfAccessTokenIsInvalid() {
			// Arrange
			var endpoint = new MessageReceivingEndpoint("http://live.com/path/?a=b", HttpDeliveryMethods.GetRequest);
			var request = new AuthorizedTokenRequest(endpoint, new Version("1.0"));

			var webWorker = new Mock<IOAuthWebWorker>(MockBehavior.Strict);
			webWorker.Setup(w => w.ProcessUserAuthorizationAsync(It.IsAny<HttpContextBase>(), CancellationToken.None)).Returns(Task.FromResult<AccessTokenResponse>(null)).Verifiable();

			var client = new MockOAuthClient(webWorker.Object);
			var context = new Mock<HttpContextBase>();

			// Act
			AuthenticationResult result = await client.VerifyAuthenticationAsync(context.Object);

			// Assert
			webWorker.Verify();

			Assert.False(result.IsSuccessful);
		}

		[TestCase]
		public async Task VerifyAuthenticationSucceeds() {
			// Arrange
			var endpoint = new MessageReceivingEndpoint("http://live.com/path/?a=b", HttpDeliveryMethods.GetRequest);
			var request = new AuthorizedTokenRequest(endpoint, new Version("1.0"));

			var webWorker = new Mock<IOAuthWebWorker>(MockBehavior.Strict);
			webWorker
				.Setup(w => w.ProcessUserAuthorizationAsync(It.IsAny<HttpContextBase>(), CancellationToken.None))
				.Returns(Task.FromResult(new AccessTokenResponse("ok", "secret", new NameValueCollection()))).Verifiable();

			var client = new MockOAuthClient(webWorker.Object);
			var context = new Mock<HttpContextBase>();

			// Act
			AuthenticationResult result = await client.VerifyAuthenticationAsync(context.Object);

			// Assert
			webWorker.Verify();

			Assert.True(result.IsSuccessful);
			Assert.AreEqual("mockoauth", result.Provider);
			Assert.AreEqual("12345", result.ProviderUserId);
			Assert.AreEqual("super", result.UserName);
			Assert.IsNotNull(result.ExtraData);
			Assert.AreEqual("ok", result.ExtraData["accesstoken"]);
		}

		private class MockOAuthClient : OAuthClient {
			/// <summary>
			/// Initializes a new instance of the <see cref="MockOAuthClient"/> class.
			/// </summary>
			public MockOAuthClient()
				: this(new Mock<IOAuthWebWorker>().Object) {
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="MockOAuthClient"/> class.
			/// </summary>
			/// <param name="worker">The worker.</param>
			public MockOAuthClient(IOAuthWebWorker worker)
				: base("mockoauth", worker) {
			}

			protected override Task<AuthenticationResult> VerifyAuthenticationCoreAsync(AccessTokenResponse response, CancellationToken cancellationToken) {
				if (response.AccessToken.Token == "ok") {
					return Task.FromResult(new AuthenticationResult(true, "mockoauth", "12345", "super", response.ExtraData));
				}

				return Task.FromResult(AuthenticationResult.Failed);
			}
		}
	}
}
