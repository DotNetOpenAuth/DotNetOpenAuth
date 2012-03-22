//-----------------------------------------------------------------------
// <copyright file="HostProcessedRequestTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using System;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using NUnit.Framework;

	[TestFixture]
	public class HostProcessedRequestTests : OpenIdTestBase {
		private Protocol protocol;
		private OpenIdProvider provider;
		private CheckIdRequest checkIdRequest;
		private AuthenticationRequest request;

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			this.protocol = Protocol.Default;
			this.provider = this.CreateProvider();
			this.checkIdRequest = new CheckIdRequest(this.protocol.Version, OPUri, DotNetOpenAuth.OpenId.AuthenticationRequestMode.Setup);
			this.checkIdRequest.Realm = RPRealmUri;
			this.checkIdRequest.ReturnTo = RPUri;
			this.request = new AuthenticationRequest(this.provider, this.checkIdRequest);
		}

		[Test]
		public void IsReturnUrlDiscoverableNoResponse() {
			Assert.AreEqual(RelyingPartyDiscoveryResult.NoServiceDocument, this.request.IsReturnUrlDiscoverable(this.provider.Channel.WebRequestHandler));
		}

		[Test]
		public void IsReturnUrlDiscoverableValidResponse() {
			this.MockResponder.RegisterMockRPDiscovery();
			this.request = new AuthenticationRequest(this.provider, this.checkIdRequest);
			Assert.AreEqual(RelyingPartyDiscoveryResult.Success, this.request.IsReturnUrlDiscoverable(this.provider.Channel.WebRequestHandler));
		}

		/// <summary>
		/// Verifies that when discovery would be performed over standard HTTP and RequireSsl
		/// is set, that discovery fails.
		/// </summary>
		[Test]
		public void IsReturnUrlDiscoverableNotSsl() {
			this.provider.SecuritySettings.RequireSsl = true;
			this.MockResponder.RegisterMockRPDiscovery();
			Assert.AreEqual(RelyingPartyDiscoveryResult.NoServiceDocument, this.request.IsReturnUrlDiscoverable(this.provider.Channel.WebRequestHandler));
		}

		/// <summary>
		/// Verifies that when discovery would be performed over HTTPS that discovery succeeds.
		/// </summary>
		[Test]
		public void IsReturnUrlDiscoverableRequireSsl() {
			this.MockResponder.RegisterMockRPDiscovery();
			this.checkIdRequest.Realm = RPRealmUriSsl;
			this.checkIdRequest.ReturnTo = RPUriSsl;

			// Try once with RequireSsl
			this.provider.SecuritySettings.RequireSsl = true;
			this.request = new AuthenticationRequest(this.provider, this.checkIdRequest);
			Assert.AreEqual(RelyingPartyDiscoveryResult.Success, this.request.IsReturnUrlDiscoverable(this.provider.Channel.WebRequestHandler));

			// And again without RequireSsl
			this.provider.SecuritySettings.RequireSsl = false;
			this.request = new AuthenticationRequest(this.provider, this.checkIdRequest);
			Assert.AreEqual(RelyingPartyDiscoveryResult.Success, this.request.IsReturnUrlDiscoverable(this.provider.Channel.WebRequestHandler));
		}

		[Test]
		public void IsReturnUrlDiscoverableValidButNoMatch() {
			this.MockResponder.RegisterMockRPDiscovery();
			this.provider.SecuritySettings.RequireSsl = false; // reset for another failure test case
			this.checkIdRequest.ReturnTo = new Uri("http://somerandom/host");
			this.request = new AuthenticationRequest(this.provider, this.checkIdRequest);
			Assert.AreEqual(RelyingPartyDiscoveryResult.NoMatchingReturnTo, this.request.IsReturnUrlDiscoverable(this.provider.Channel.WebRequestHandler));
		}
	}
}
