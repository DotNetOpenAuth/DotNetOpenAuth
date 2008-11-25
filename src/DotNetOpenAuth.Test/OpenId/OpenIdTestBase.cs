//-----------------------------------------------------------------------
// <copyright file="OpenIdTestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOpenAuth.Test.Mocks;

	public class OpenIdTestBase : TestBase {
		protected RelyingPartySecuritySettings RelyingPartySecuritySettings { get; private set; }

		protected ProviderSecuritySettings ProviderSecuritySettings { get; private set; }

		internal TestWebRequestHandler requestHandler;
		internal MockHttpRequest mockResponder;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.RelyingPartySecuritySettings = RelyingPartySection.Configuration.SecuritySettings.CreateSecuritySettings();
			this.ProviderSecuritySettings = ProviderSection.Configuration.SecuritySettings.CreateSecuritySettings();

			this.requestHandler = new TestWebRequestHandler();
			this.mockResponder = new MockHttpRequest(requestHandler);
		}
	}
}
