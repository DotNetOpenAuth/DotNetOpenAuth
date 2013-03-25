//-----------------------------------------------------------------------
// <copyright file="HostProcessedRequestTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.Test.Mocks;

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
			this.checkIdRequest = new CheckIdRequest(this.protocol.Version, OPUri, AuthenticationRequestMode.Setup);
			this.checkIdRequest.Realm = RPRealmUri;
			this.checkIdRequest.ReturnTo = RPUri;
			this.request = new AuthenticationRequest(this.provider, this.checkIdRequest);
		}

		[Test]
		public async Task IsReturnUrlDiscoverableNoResponse() {
			Assert.AreEqual(RelyingPartyDiscoveryResult.NoServiceDocument, await this.request.IsReturnUrlDiscoverableAsync(this.provider.Channel.HostFactories, CancellationToken.None));
		}

		[Test]
		public async Task IsReturnUrlDiscoverableValidResponse() {
			this.RegisterMockRPDiscovery(false);

			this.request = new AuthenticationRequest(this.provider, this.checkIdRequest);
			Assert.AreEqual(RelyingPartyDiscoveryResult.Success, await this.request.IsReturnUrlDiscoverableAsync(this.provider.Channel.HostFactories, CancellationToken.None));
		}

		/// <summary>
		/// Verifies that when discovery would be performed over standard HTTP and RequireSsl
		/// is set, that discovery fails.
		/// </summary>
		[Test]
		public async Task IsReturnUrlDiscoverableNotSsl() {
			this.RegisterMockRPDiscovery(false);
			this.provider.SecuritySettings.RequireSsl = true;
			Assert.AreEqual(RelyingPartyDiscoveryResult.NoServiceDocument, await this.request.IsReturnUrlDiscoverableAsync(this.provider.Channel.HostFactories, CancellationToken.None));
		}

		/// <summary>
		/// Verifies that when discovery would be performed over HTTPS that discovery succeeds.
		/// </summary>
		[Test]
		public async Task IsReturnUrlDiscoverableRequireSsl() {
			this.RegisterMockRPDiscovery(ssl: false);
			this.RegisterMockRPDiscovery(ssl: true);
			this.checkIdRequest.Realm = RPRealmUriSsl;
			this.checkIdRequest.ReturnTo = RPUriSsl;

			// Try once with RequireSsl
			this.provider.SecuritySettings.RequireSsl = true;
			this.request = new AuthenticationRequest(this.provider, this.checkIdRequest);
			Assert.AreEqual(RelyingPartyDiscoveryResult.Success, await this.request.IsReturnUrlDiscoverableAsync(this.HostFactories, CancellationToken.None));

			// And again without RequireSsl
			this.provider.SecuritySettings.RequireSsl = false;
			this.request = new AuthenticationRequest(this.provider, this.checkIdRequest);
			Assert.AreEqual(RelyingPartyDiscoveryResult.Success, await this.request.IsReturnUrlDiscoverableAsync(this.HostFactories, CancellationToken.None));
		}

		[Test]
		public async Task IsReturnUrlDiscoverableValidButNoMatch() {
			this.RegisterMockRPDiscovery(false);
			this.provider.SecuritySettings.RequireSsl = false; // reset for another failure test case
			this.checkIdRequest.ReturnTo = new Uri("http://somerandom/host");
			this.request = new AuthenticationRequest(this.provider, this.checkIdRequest);
			Assert.AreEqual(
				RelyingPartyDiscoveryResult.NoMatchingReturnTo,
				await this.request.IsReturnUrlDiscoverableAsync(this.provider.Channel.HostFactories, CancellationToken.None));
		}
	}
}
