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

	using NUnit.Framework;

	[TestFixture]
	public class ResourceServerTests : OAuth2TestBase {
		[Test]
		public void GetAccessTokenWithMissingAccessToken() {
			var rsa = new RSACryptoServiceProvider(512);
			var resourceServer = new ResourceServer(new StandardAccessTokenAnalyzer(rsa, rsa));

			var requestHeaders = new NameValueCollection {
				{ "Authorization", "Bearer " },
			};
			var request = new HttpRequestInfo("GET", new Uri("http://localhost/resource"), headers: requestHeaders);
			Assert.That(() => resourceServer.GetAccessToken(request), Throws.InstanceOf<ProtocolException>());
		}

		[Test]
		public void GetPrincipalWithMissingAccessToken() {
			var rsa = new RSACryptoServiceProvider(512);
			var resourceServer = new ResourceServer(new StandardAccessTokenAnalyzer(rsa, rsa));

			var requestHeaders = new NameValueCollection {
				{ "Authorization", "Bearer " },
			};
			var request = new HttpRequestInfo("GET", new Uri("http://localhost/resource"), headers: requestHeaders);
			Assert.That(() => resourceServer.GetPrincipal(request), Throws.InstanceOf<ProtocolException>());
		}

		[Test]
		public void GetAccessTokenWithCorruptedToken() {
			var rsa = new RSACryptoServiceProvider(512);
			var resourceServer = new ResourceServer(new StandardAccessTokenAnalyzer(rsa, rsa));

			var requestHeaders = new NameValueCollection {
				{ "Authorization", "Bearer foobar" },
			};
			var request = new HttpRequestInfo("GET", new Uri("http://localhost/resource"), headers: requestHeaders);
			Assert.That(() => resourceServer.GetAccessToken(request), Throws.InstanceOf<ProtocolException>());
		}
	}
}
