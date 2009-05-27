//-----------------------------------------------------------------------
// <copyright file="AnonymousRequestTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AnonymousRequestTests : OpenIdTestBase {
		/// <summary>
		/// Verifies that IsApproved controls which response message is returned.
		/// </summary>
		[TestMethod]
		public void IsApprovedDeterminesReturnedMessage() {
			var op = CreateProvider();
			Protocol protocol = Protocol.V20;
			var req = new SignedResponseRequest(protocol.Version, OPUri, AuthenticationRequestMode.Setup);
			req.ReturnTo = RPUri;
			var anonReq = new AnonymousRequest(op, req);

			Assert.IsFalse(anonReq.IsApproved.HasValue);

			anonReq.IsApproved = false;
			Assert.IsInstanceOfType(anonReq.Response, typeof(NegativeAssertionResponse));

			anonReq.IsApproved = true;
			Assert.IsInstanceOfType(anonReq.Response, typeof(IndirectSignedResponse));
			Assert.IsNotInstanceOfType(anonReq.Response, typeof(PositiveAssertionResponse));
		}
	}
}
