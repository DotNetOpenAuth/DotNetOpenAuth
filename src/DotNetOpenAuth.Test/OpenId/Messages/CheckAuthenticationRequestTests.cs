//-----------------------------------------------------------------------
// <copyright file="CheckAuthenticationRequestTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using NUnit.Framework;

	[TestFixture]
	public class CheckAuthenticationRequestTests : OpenIdTestBase {
		/// <summary>
		/// Verifies that the check_auth request is sent preserving EXACTLY (non-normalized)
		/// what is in the positive assertion.
		/// </summary>
		/// <remarks>
		/// This is very important because any normalization
		/// (like changing https://host:443/ to https://host/) in the message will invalidate the signature
		/// and cause the authentication to inappropriately fail.
		/// Designed to verify fix to Trac #198.
		/// </remarks>
		[Test]
		public void ExactPositiveAssertionPreservation() {
			var rp = CreateRelyingParty(true);

			// Initialize the positive assertion response with some data that is NOT in normalized form.
			var positiveAssertion = new PositiveAssertionResponse(Protocol.Default.Version, RPUri)
			{
				ClaimedIdentifier = "https://HOST:443/a",
				ProviderEndpoint = new Uri("https://anotherHOST:443/b"),
			};

			var checkAuth = new CheckAuthenticationRequest(positiveAssertion, rp.Channel);
			var actual = rp.Channel.MessageDescriptions.GetAccessor(checkAuth);
			Assert.AreEqual("https://HOST:443/a", actual["openid.claimed_id"]);
			Assert.AreEqual("https://anotherHOST:443/b", actual["openid.op_endpoint"]);
		}
	}
}
