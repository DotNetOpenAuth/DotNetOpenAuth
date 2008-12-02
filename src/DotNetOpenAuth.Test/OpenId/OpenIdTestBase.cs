//-----------------------------------------------------------------------
// <copyright file="OpenIdTestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	public class OpenIdTestBase : TestBase {
		internal IDirectSslWebRequestHandler RequestHandler;

		internal MockHttpRequest MockResponder;

		protected RelyingPartySecuritySettings RelyingPartySecuritySettings { get; private set; }

		protected ProviderSecuritySettings ProviderSecuritySettings { get; private set; }

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.RelyingPartySecuritySettings = RelyingPartySection.Configuration.SecuritySettings.CreateSecuritySettings();
			this.ProviderSecuritySettings = ProviderSection.Configuration.SecuritySettings.CreateSecuritySettings();

			this.MockResponder = MockHttpRequest.CreateUntrustedMockHttpHandler();
			this.RequestHandler = this.MockResponder.MockWebRequestHandler;
		}
	}
}
