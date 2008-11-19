//-----------------------------------------------------------------------
// <copyright file="OpenIdTestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public class OpenIdTestBase : TestBase {
		protected RelyingPartySecuritySettings RelyingPartySecuritySettings { get; private set; }

		protected ProviderSecuritySettings ProviderSecuritySettings { get; private set; }

		public override void SetUp() {
			base.SetUp();

			RelyingPartySecuritySettings = RelyingPartySection.Configuration.SecuritySettings.CreateSecuritySettings();
			ProviderSecuritySettings = ProviderSection.Configuration.SecuritySettings.CreateSecuritySettings();
		}
	}
}
