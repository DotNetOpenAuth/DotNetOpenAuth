//-----------------------------------------------------------------------
// <copyright file="OAuthClientTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Test {
	using System;
	using System.Web;
	using DotNetOpenAuth.AspNet;
	using DotNetOpenAuth.AspNet.Clients;
	using DotNetOpenAuth.Messaging;
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
		public void RequestAuthenticationInvokeMethodOnWebWorker() {
			// Arrange
			var webWorker = new Mock<IOAuthWebWorker>(MockBehavior.Strict);
			webWorker
				.Setup(w => w.RequestAuthentication(It.Is<Uri>(u => u.ToString().Equals("http://live.com/my/path.cshtml?q=one"))))
				.Verifiable();

			var client = new MockOAuthClient(webWorker.Object);
			var returnUri = new Uri("http://live.com/my/path.cshtml?q=one");
			var context = new Mock<HttpContextBase>();

			// Act
			client.RequestAuthentication(context.Object, returnUri);

			// Assert
			webWorker.Verify();
		}

		[TestCase]
		public void VerifyAuthenticationFailsIfResponseTokenIsNull() {
			// Arrange
			var webWorker = new Mock<IOAuthWebWorker>(MockBehavior.Strict);
			webWorker.Setup(w => w.ProcessUserAuthorization()).Returns((AuthorizedTokenResponse)null);

			var client = new MockOAuthClient(webWorker.Object);
			var context = new Mock<HttpContextBase>();

			// Act
			client.VerifyAuthentication(context.Object);

			// Assert
			webWorker.Verify();
		}

		[TestCase]
		public void VerifyAuthenticationFailsIfAccessTokenIsInvalid() {
			// Arrange
			var endpoint = new MessageReceivingEndpoint("http://live.com/path/?a=b", HttpDeliveryMethods.GetRequest);
			var request = new AuthorizedTokenRequest(endpoint, new Version("1.0"));
			var response = new AuthorizedTokenResponse(request) {
				AccessToken = "invalid token"
			};

			var webWorker = new Mock<IOAuthWebWorker>(MockBehavior.Strict);
			webWorker.Setup(w => w.ProcessUserAuthorization()).Returns(response).Verifiable();

			var client = new MockOAuthClient(webWorker.Object);
			var context = new Mock<HttpContextBase>();

			// Act
			AuthenticationResult result = client.VerifyAuthentication(context.Object);

			// Assert
			webWorker.Verify();

			Assert.False(result.IsSuccessful);
		}

		[TestCase]
		public void VerifyAuthenticationSucceeds() {
			// Arrange
			var endpoint = new MessageReceivingEndpoint("http://live.com/path/?a=b", HttpDeliveryMethods.GetRequest);
			var request = new AuthorizedTokenRequest(endpoint, new Version("1.0"));
			var response = new AuthorizedTokenResponse(request) {
				AccessToken = "ok"
			};

			var webWorker = new Mock<IOAuthWebWorker>(MockBehavior.Strict);
			webWorker.Setup(w => w.ProcessUserAuthorization()).Returns(response).Verifiable();

			var client = new MockOAuthClient(webWorker.Object);
			var context = new Mock<HttpContextBase>();

			// Act
			AuthenticationResult result = client.VerifyAuthentication(context.Object);

			// Assert
			webWorker.Verify();

			Assert.True(result.IsSuccessful);
			Assert.AreEqual("mockoauth", result.Provider);
			Assert.AreEqual("12345", result.ProviderUserId);
			Assert.AreEqual("super", result.UserName);
			Assert.IsNotNull(result.ExtraData);
			Assert.IsTrue(result.ExtraData.ContainsKey("accesstoken"));
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

			protected override AuthenticationResult VerifyAuthenticationCore(AuthorizedTokenResponse response) {
				if (response.AccessToken == "ok") {
					return new AuthenticationResult(true, "mockoauth", "12345", "super", response.ExtraData);
				}

				return AuthenticationResult.Failed;
			}
		}
	}
}
