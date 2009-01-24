//-----------------------------------------------------------------------
// <copyright file="AuthenticationRequestTest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AuthenticationRequestTest : OpenIdTestBase {
		[TestMethod]
		public void IsReturnUrlDiscoverable() {
			Protocol protocol = Protocol.Default;
			OpenIdProvider provider = this.CreateProvider();
			CheckIdRequest checkIdRequest = new CheckIdRequest(protocol.Version, ProviderUri, DotNetOpenAuth.OpenId.RelyingParty.AuthenticationRequestMode.Setup);
			checkIdRequest.Realm = TestSupport.Realm;
			checkIdRequest.ReturnTo = TestSupport.ReturnTo;
			AuthenticationRequest request = new AuthenticationRequest(provider, checkIdRequest);
			Assert.IsFalse(request.IsReturnUrlDiscoverable);

			this.MockResponder.RegisterMockRPDiscovery();
			request = new AuthenticationRequest(provider, checkIdRequest);
			Assert.IsTrue(request.IsReturnUrlDiscoverable);
		}
	}
}
