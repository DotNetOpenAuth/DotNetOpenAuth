﻿//-----------------------------------------------------------------------
// <copyright file="AnonymousRequestTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class AnonymousRequestTests : OpenIdTestBase {
		/// <summary>
		/// Verifies that IsApproved controls which response message is returned.
		/// </summary>
		[Test]
		public void IsApprovedDeterminesReturnedMessage() {
			var op = CreateProvider();
			Protocol protocol = Protocol.V20;
			var req = new SignedResponseRequest(protocol.Version, OPUri, AuthenticationRequestMode.Setup);
			req.ReturnTo = RPUri;
			var anonReq = new AnonymousRequest(op, req);

			Assert.IsFalse(anonReq.IsApproved.HasValue);

			anonReq.IsApproved = false;
			Assert.IsInstanceOf<NegativeAssertionResponse>(anonReq.Response);

			anonReq.IsApproved = true;
			Assert.IsInstanceOf<IndirectSignedResponse>(anonReq.Response);
			Assert.IsNotInstanceOf<PositiveAssertionResponse>(anonReq.Response);
		}
	}
}
