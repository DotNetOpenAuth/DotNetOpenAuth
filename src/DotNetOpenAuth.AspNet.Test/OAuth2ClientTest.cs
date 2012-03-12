//-----------------------------------------------------------------------
// <copyright file="OAuth2ClientTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Test {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
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
		public void RequestAuthenticationIssueCorrectRedirect() {
			// Arrange
			var client = new MockOAuth2Client();
			var context = new Mock<HttpContextBase>(MockBehavior.Strict);
			context.Setup(c => c.Response.Redirect("http://live.com/?q=http://return.to.me/", true)).Verifiable();

			// Act
			client.RequestAuthentication(context.Object, new Uri("http://return.to.me"));

			// Assert
			context.Verify();
		}

		[TestCase]
		public void VerifyAuthenticationThrowsIfContextIsNull() {
			// Arrange
			var client = new MockOAuth2Client();

			// Act && Assert
			Assert.Throws<ArgumentNullException>(() => client.VerifyAuthentication(null));
		}

		[TestCase]
		public void VerifyAuthenticationFailsIfCodeIsNotPresent() {
			// Arrange
			var client = new MockOAuth2Client();
			var context = new Mock<HttpContextBase>(MockBehavior.Strict);
			var queryStrings = new NameValueCollection();
			context.Setup(c => c.Request.QueryString).Returns(queryStrings);

			// Act 
			AuthenticationResult result = client.VerifyAuthentication(context.Object);

			// Assert
			Assert.IsFalse(result.IsSuccessful);
		}

		[TestCase]
		public void VerifyAuthenticationFailsIfAccessTokenIsNull() {
			// Arrange
			var client = new MockOAuth2Client();
			var context = new Mock<HttpContextBase>(MockBehavior.Strict);
			var queryStrings = new NameValueCollection();
			queryStrings.Add("code", "random");
			context.Setup(c => c.Request.QueryString).Returns(queryStrings);

			// Act 
			AuthenticationResult result = client.VerifyAuthentication(context.Object);

			// Assert
			Assert.IsFalse(result.IsSuccessful);
		}

		[TestCase]
		public void VerifyAuthenticationSucceeds() {
			// Arrange
			var client = new MockOAuth2Client();
			var context = new Mock<HttpContextBase>(MockBehavior.Strict);
			var queryStrings = new NameValueCollection();
			queryStrings.Add("code", "secret");
			context.Setup(c => c.Request.QueryString).Returns(queryStrings);

			// Act 
			AuthenticationResult result = client.VerifyAuthentication(context.Object);

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

			protected override IDictionary<string, string> GetUserData(string accessToken) {
				if (accessToken == "abcde") {
					return new Dictionary<string, string>
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
