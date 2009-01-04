//-----------------------------------------------------------------------
// <copyright file="AuthenticationRequestTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AuthenticationRequestTests : OpenIdTestBase {
		private readonly Realm realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
		private readonly Uri returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
		}

		/// <summary>
		/// Verifies the Provider property returns non-null.
		/// </summary>
		[TestMethod]
		public void Provider() {
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					Identifier id = this.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
					IAuthenticationRequest request = rp.CreateRequest(id, this.realm, this.returnTo);
					Assert.IsNotNull(request.Provider);
				},
				TestSupport.AutoProvider);
		}
	}
}
