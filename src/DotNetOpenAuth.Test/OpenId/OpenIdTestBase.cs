//-----------------------------------------------------------------------
// <copyright file="OpenIdTestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	public class OpenIdTestBase : TestBase {
		internal IDirectSslWebRequestHandler RequestHandler;

		internal MockHttpRequest MockResponder;

		protected internal static readonly Uri ProviderUri = new Uri("http://provider");
		protected internal static readonly Uri RPUri = new Uri("http://rp");

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

		protected Identifier GetMockIdentifier(TestSupport.Scenarios scenario, ProtocolVersion providerVersion) {
			return this.GetMockIdentifier(scenario, providerVersion, false);
		}

		protected Identifier GetMockIdentifier(TestSupport.Scenarios scenario, ProtocolVersion providerVersion, bool useSsl) {
			return TestSupport.GetMockIdentifier(scenario, this.MockResponder, providerVersion, useSsl);
		}

		/// <summary>
		/// Creates a standard <see cref="OpenIdRelyingParty"/> instance for general testing.
		/// </summary>
		/// <returns>The new instance.</returns>
		protected OpenIdRelyingParty CreateRelyingParty() {
			var rp = new OpenIdRelyingParty(new AssociationMemoryStore<Uri>(), new NonceMemoryStore(TimeSpan.FromMinutes(5)), new PrivateSecretMemoryStore());
			rp.Channel.WebRequestHandler = this.MockResponder.MockWebRequestHandler;
			return rp;
		}
	}
}
