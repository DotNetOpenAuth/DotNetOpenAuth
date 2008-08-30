using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetOAuth.Test {
	[TestClass]
	public class ServiceProviderTest : TestBase {
		/// <summary>
		/// A test for UserAuthorizationUri
		/// </summary>
		[TestMethod]
		public void UserAuthorizationUriTest() {
			ServiceProvider target = new ServiceProvider();
			Uri expected = new Uri("http://localhost/authorization");
			Uri actual;
			target.UserAuthorizationUri = expected;
			actual = target.UserAuthorizationUri;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		/// A test for RequestTokenUri
		/// </summary>
		[TestMethod()]
		public void RequestTokenUriTest() {
			ServiceProvider target = new ServiceProvider();
			Uri expected = new Uri("http://localhost/requesttoken");
			Uri actual;
			target.RequestTokenUri = expected;
			actual = target.RequestTokenUri;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		/// Verifies that oauth parameters are not allowed in <see cref="ServiceProvider.RequestTokenUri"/>,
		/// per section OAuth 1.0 section 4.1.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void RequestTokenUriWithOAuthParametersTest() {
			ServiceProvider target = new ServiceProvider();
			target.RequestTokenUri = new Uri("http://localhost/requesttoken?oauth_token=something");
		}

		/// <summary>
		/// A test for AccessTokenUri
		/// </summary>
		[TestMethod()]
		public void AccessTokenUriTest() {
			ServiceProvider target = new ServiceProvider();
			Uri expected = new Uri("http://localhost/accesstoken");
			Uri actual;
			target.AccessTokenUri = expected;
			actual = target.AccessTokenUri;
			Assert.AreEqual(expected, actual);
		}
	}
}
