//-----------------------------------------------------------------------
// <copyright file="PositiveAnonymousResponseTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class PositiveAnonymousResponseTests : OpenIdTestBase {
		private readonly Realm realm = new Realm("http://localhost/rp.aspx");
		private readonly Uri returnTo = new Uri("http://localhost/rp.aspx");

		[SetUp]
		public override void SetUp() {
			base.SetUp();
		}

		/// <summary>
		/// Verifies that the Status property returns the correct value.
		/// </summary>
		[Test]
		public void CtorAndProperties() {
			var responseMessage = new IndirectSignedResponse(Protocol.V20.Version, this.returnTo);
			var ext = new ClaimsResponse();
			responseMessage.Extensions.Add(ext);
			var response = new PositiveAnonymousResponse(responseMessage);
			Assert.AreEqual(AuthenticationStatus.ExtensionsOnly, response.Status);
			Assert.AreSame(responseMessage, response.Response);
			Assert.IsNull(response.ClaimedIdentifier);
			Assert.IsNull(response.FriendlyIdentifierForDisplay);
			Assert.IsNull(response.Exception);
			Assert.IsNull(response.Provider);
			Assert.AreSame(ext, response.GetUntrustedExtension<ClaimsResponse>());
		}

		/// <summary>
		/// Verifies the Provider property.
		/// </summary>
		[Test]
		public void ProviderTest() {
			var responseMessage = new IndirectSignedResponse(Protocol.V20.Version, this.returnTo);
			responseMessage.ProviderEndpoint = OPUri;
			var response = new PositiveAnonymousResponse(responseMessage);
			Assert.IsNotNull(response.Provider);
			Assert.AreEqual(OPUri, response.Provider.Uri);
			Assert.AreEqual(responseMessage.Version, response.Provider.Version);
		}
	}
}
