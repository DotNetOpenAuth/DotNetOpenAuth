//-----------------------------------------------------------------------
// <copyright file="OAuth2ClientTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Test {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.AspNet.Clients;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class OAuth2ClientTest {
		[TestCase]
		public void TestProviderName() {
			// Arrange
			var client = new MockOAuth2Client();

			// Act
			string providerName = client.ProviderName;

			// Assert
			Assert.AreEqual("mockprovider", providerName);
		}

		[TestCase]
		public async Task RequestAuthenticationIssueCorrectRedirect() {
			// Arrange
			var client = new MockOAuth2Client();
			var context = new Mock<HttpContextBase>(MockBehavior.Strict);
			context.Setup(c => c.Response.Redirect("http://live.com/?q=http://return.to.me/", true)).Verifiable();

			// Act
			await client.RequestAuthenticationAsync(context.Object, new Uri("http://return.to.me"));

			// Assert
			context.Verify();
		}

		[TestCase]
		public void VerifyAuthenticationThrowsIfContextIsNull() {
			// Arrange
			var client = new MockOAuth2Client();

			// Act && Assert
			Assert.Throws<ArgumentNullException>(() => client.VerifyAuthenticationAsync(null, new Uri("http://me.com")).GetAwaiter().GetResult());
		}

		[TestCase]
		public void VerifyAuthenticationWithoutReturnUrlThrows() {
			// Arrange
			var client = new MockOAuth2Client();

			// Act && Assert
			Assert.Throws<InvalidOperationException>(() => client.VerifyAuthenticationAsync(new Mock<HttpContextBase>().Object).GetAwaiter().GetResult());
		}

		[TestCase]
		public async Task VerifyAuthenticationFailsIfCodeIsNotPresent() {
			// Arrange
			var client = new MockOAuth2Client();
			var context = new Mock<HttpContextBase>(MockBehavior.Strict);
			var queryStrings = new NameValueCollection();
			context.Setup(c => c.Request.QueryString).Returns(queryStrings);

			// Act 
			AuthenticationResult result = await client.VerifyAuthenticationAsync(context.Object, new Uri("http://me.com"));

			// Assert
			Assert.IsFalse(result.IsSuccessful);
		}

		[TestCase]
		public async Task VerifyAuthenticationFailsIfAccessTokenIsNull() {
			// Arrange
			var client = new MockOAuth2Client();
			var context = new Mock<HttpContextBase>(MockBehavior.Strict);
			var queryStrings = new NameValueCollection();
			queryStrings.Add("code", "random");
			context.Setup(c => c.Request.QueryString).Returns(queryStrings);

			// Act 
			AuthenticationResult result = await client.VerifyAuthenticationAsync(context.Object, new Uri("http://me.com"));

			// Assert
			Assert.IsFalse(result.IsSuccessful);
		}

		[TestCase]
		public async Task VerifyAuthenticationSucceeds() {
			// Arrange
			var client = new MockOAuth2Client();
			var context = new Mock<HttpContextBase>(MockBehavior.Strict);
			var queryStrings = new NameValueCollection();
			queryStrings.Add("code", "secret");
			context.Setup(c => c.Request.QueryString).Returns(queryStrings);

			// Act 
			AuthenticationResult result = await client.VerifyAuthenticationAsync(context.Object, new Uri("http://me.com"));

			// Assert
			Assert.True(result.IsSuccessful);
			Assert.AreEqual("mockprovider", result.Provider);
			Assert.AreEqual("12345", result.ProviderUserId);
			Assert.AreEqual("John Doe", result.UserName);
			Assert.NotNull(result.ExtraData);
			Assert.AreEqual("abcde", result.ExtraData["accesstoken"]);
		}

		private class MockOAuth2Client : OAuth2Client {
			public MockOAuth2Client()
				: base("mockprovider") {
			}

			protected override Uri GetServiceLoginUrl(Uri returnUrl) {
				string url = "http://live.com/?q=" + returnUrl.ToString();
				return new Uri(url);
			}

			protected override string QueryAccessToken(Uri returnUrl, string authorizationCode) {
				return (authorizationCode == "secret") ? "abcde" : null;
			}

			protected override NameValueCollection GetUserData(string accessToken) {
				if (accessToken == "abcde") {
					return new NameValueCollection
					{
						{ "id", "12345" },
						{ "name", "John Doe" },
					};
				}

				return null;
			}
		}
	}
}
