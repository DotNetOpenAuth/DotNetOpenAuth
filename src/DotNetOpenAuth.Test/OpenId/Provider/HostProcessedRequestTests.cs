//-----------------------------------------------------------------------
// <copyright file="HostProcessedRequestTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using System;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class HostProcessedRequestTests : OpenIdTestBase {
		[TestMethod]
		public void IsReturnUrlDiscoverable() {
			Protocol protocol = Protocol.Default;
			OpenIdProvider provider = this.CreateProvider();
			CheckIdRequest checkIdRequest = new CheckIdRequest(protocol.Version, OPUri, DotNetOpenAuth.OpenId.RelyingParty.AuthenticationRequestMode.Setup);
			checkIdRequest.Realm = RPRealmUri;
			checkIdRequest.ReturnTo = RPUri;
			AuthenticationRequest request = new AuthenticationRequest(provider, checkIdRequest);
			Assert.AreEqual(RelyingPartyDiscoveryResult.NoServiceDocument, request.IsReturnUrlDiscoverable(this.MockResponder.MockWebRequestHandler));

			this.MockResponder.RegisterMockRPDiscovery();
			request = new AuthenticationRequest(provider, checkIdRequest);
			Assert.AreEqual(RelyingPartyDiscoveryResult.Success, request.IsReturnUrlDiscoverable(this.MockResponder.MockWebRequestHandler));

			checkIdRequest.ReturnTo = new Uri("http://somerandom/host");
			request = new AuthenticationRequest(provider, checkIdRequest);
			Assert.AreEqual(RelyingPartyDiscoveryResult.NoMatchingReturnTo, request.IsReturnUrlDiscoverable(this.MockResponder.MockWebRequestHandler));
		}
	}
}
