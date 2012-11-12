//-----------------------------------------------------------------------
// <copyright file="ResourceServerTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class ResourceServerTests : OAuth2TestBase {
		[Test]
		public void GetAccessTokenWithMissingAccessToken() {
			var resourceServer = new ResourceServer(new StandardAccessTokenAnalyzer(AsymmetricKey, null));

			var requestHeaders = new NameValueCollection {
				{ "Authorization", "Bearer " },
			};
			var request = new HttpRequestInfo("GET", new Uri("http://localhost/resource"), headers: requestHeaders);
			Assert.That(() => resourceServer.GetAccessToken(request), Throws.InstanceOf<ProtocolException>());
		}

		[Test]
		public void GetPrincipalWithMissingAccessToken() {
			var resourceServer = new ResourceServer(new StandardAccessTokenAnalyzer(AsymmetricKey, null));

			var requestHeaders = new NameValueCollection {
				{ "Authorization", "Bearer " },
			};
			var request = new HttpRequestInfo("GET", new Uri("http://localhost/resource"), headers: requestHeaders);
			Assert.That(() => resourceServer.GetPrincipal(request), Throws.InstanceOf<ProtocolException>());
		}

		[Test]
		public void GetAccessTokenWithTotallyFakeToken() {
			var resourceServer = new ResourceServer(new StandardAccessTokenAnalyzer(AsymmetricKey, null));

			var requestHeaders = new NameValueCollection {
				{ "Authorization", "Bearer foobar" },
			};
			var request = new HttpRequestInfo("GET", new Uri("http://localhost/resource"), headers: requestHeaders);
			Assert.That(() => resourceServer.GetAccessToken(request), Throws.InstanceOf<ProtocolException>());
		}

		[Test]
		public void GetAccessTokenWithCorruptedToken() {
			var accessToken = this.ObtainValidAccessToken();

			var resourceServer = new ResourceServer(new StandardAccessTokenAnalyzer(AsymmetricKey, null));

			var requestHeaders = new NameValueCollection {
				{ "Authorization", "Bearer " + accessToken.Substring(0, accessToken.Length - 1) + "zzz" },
			};
			var request = new HttpRequestInfo("GET", new Uri("http://localhost/resource"), headers: requestHeaders);
			Assert.That(() => resourceServer.GetAccessToken(request), Throws.InstanceOf<ProtocolException>());
		}

		[Test]
		public void GetAccessTokenWithValidToken() {
			var accessToken = this.ObtainValidAccessToken();

			var resourceServer = new ResourceServer(new StandardAccessTokenAnalyzer(AsymmetricKey, null));

			var requestHeaders = new NameValueCollection {
				{ "Authorization", "Bearer " +  accessToken },
			};
			var request = new HttpRequestInfo("GET", new Uri("http://localhost/resource"), headers: requestHeaders);
			var resourceServerDecodedToken = resourceServer.GetAccessToken(request);
			Assert.That(resourceServerDecodedToken, Is.Not.Null);
		}

		private string ObtainValidAccessToken() {
			string accessToken = null;
			var authServer = CreateAuthorizationServerMock();
			authServer.Setup(
				a => a.IsAuthorizationValid(It.Is<IAuthorizationDescription>(d => d.User == null && d.ClientIdentifier == ClientId && MessagingUtilities.AreEquivalent(d.Scope, TestScopes))))
				.Returns(true);
			authServer.Setup(
				a => a.TryAuthorizeClientCredentialsGrant(It.Is<IAccessTokenRequest>(d => d.ClientIdentifier == ClientId && MessagingUtilities.AreEquivalent(d.Scope, TestScopes))))
				.Returns(true);
			var coordinator = new OAuth2Coordinator<WebServerClient>(
				AuthorizationServerDescription,
				authServer.Object,
				new WebServerClient(AuthorizationServerDescription),
				client => {
					var authState = client.GetClientAccessToken(TestScopes);
					Assert.That(authState.AccessToken, Is.Not.Null.And.Not.Empty);
					Assert.That(authState.RefreshToken, Is.Null);
					accessToken = authState.AccessToken;
				},
				server => {
					server.HandleTokenRequest().Respond();
				});
			coordinator.Run();

			return accessToken;
		}
	}
}
