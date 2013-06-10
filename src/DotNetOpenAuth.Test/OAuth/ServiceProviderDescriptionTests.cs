//-----------------------------------------------------------------------
// <copyright file="ServiceProviderDescriptionTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using NUnit.Framework;

	/// <summary>
	/// Tests for the <see cref="ServiceProviderHostDescription"/> class.
	/// </summary>
	[TestFixture]
	public class ServiceProviderDescriptionTests : TestBase {
		/// <summary>
		/// A test for UserAuthorizationUri
		/// </summary>
		[Test]
		public void UserAuthorizationUriTest() {
			var target = new ServiceProviderHostDescription();
			var expected = new MessageReceivingEndpoint("http://localhost/authorization", HttpDeliveryMethods.GetRequest);
			MessageReceivingEndpoint actual;
			target.UserAuthorizationEndpoint = expected;
			actual = target.UserAuthorizationEndpoint;
			Assert.AreEqual(expected, actual);

			target.UserAuthorizationEndpoint = null;
			Assert.IsNull(target.UserAuthorizationEndpoint);
		}

		/// <summary>
		/// A test for RequestTokenUri
		/// </summary>
		[Test]
		public void RequestTokenUriTest() {
			var target = new ServiceProviderHostDescription();
			var expected = new MessageReceivingEndpoint("http://localhost/requesttoken", HttpDeliveryMethods.GetRequest);
			MessageReceivingEndpoint actual;
			target.RequestTokenEndpoint = expected;
			actual = target.RequestTokenEndpoint;
			Assert.AreEqual(expected, actual);

			target.RequestTokenEndpoint = null;
			Assert.IsNull(target.RequestTokenEndpoint);
		}

		/// <summary>
		/// Verifies that oauth parameters are not allowed in <see cref="ServiceProvider.RequestTokenUri"/>,
		/// per section OAuth 1.0 section 4.1.
		/// </summary>
		[Test, ExpectedException(typeof(ArgumentException))]
		public void RequestTokenUriWithOAuthParametersTest() {
			var target = new ServiceProviderHostDescription();
			target.RequestTokenEndpoint = new MessageReceivingEndpoint("http://localhost/requesttoken?oauth_token=something", HttpDeliveryMethods.GetRequest);
		}

		/// <summary>
		/// A test for AccessTokenUri
		/// </summary>
		[Test]
		public void AccessTokenUriTest() {
			var target = new ServiceProviderHostDescription();
			MessageReceivingEndpoint expected = new MessageReceivingEndpoint("http://localhost/accesstoken", HttpDeliveryMethods.GetRequest);
			MessageReceivingEndpoint actual;
			target.AccessTokenEndpoint = expected;
			actual = target.AccessTokenEndpoint;
			Assert.AreEqual(expected, actual);

			target.AccessTokenEndpoint = null;
			Assert.IsNull(target.AccessTokenEndpoint);
		}
	}
}
