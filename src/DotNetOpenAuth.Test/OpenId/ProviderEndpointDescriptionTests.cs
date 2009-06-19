//-----------------------------------------------------------------------
// <copyright file="ProviderEndpointDescriptionTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ProviderEndpointDescriptionTests : OpenIdTestBase {
		private ProviderEndpointDescription se;

		private string[] v20TypeUris = { Protocol.V20.ClaimedIdentifierServiceTypeURI };

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.se = new ProviderEndpointDescription(OPUri, Protocol.V20.Version);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void IsExtensionSupportedNullType() {
			this.se.IsExtensionSupported((Type)null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void IsExtensionSupportedNullString() {
			this.se.IsExtensionSupported((string)null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void IsExtensionSupportedEmptyString() {
			this.se.IsExtensionSupported(string.Empty);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void IsExtensionSupportedNullExtension() {
			this.se.IsExtensionSupported((IOpenIdMessageExtension)null);
		}

		[TestMethod]
		public void IsExtensionSupported() {
			this.se = new ProviderEndpointDescription(OPUri, this.v20TypeUris);
			Assert.IsFalse(this.se.IsExtensionSupported<ClaimsRequest>());
			Assert.IsFalse(this.se.IsExtensionSupported(new ClaimsRequest()));
			Assert.IsFalse(this.se.IsExtensionSupported("http://someextension/typeuri"));

			this.se = new ProviderEndpointDescription(
				OPUri,
				new[] { Protocol.V20.ClaimedIdentifierServiceTypeURI, "http://someextension", Constants.sreg_ns });
			Assert.IsTrue(this.se.IsExtensionSupported<ClaimsRequest>());
			Assert.IsTrue(this.se.IsExtensionSupported(new ClaimsRequest()));
			Assert.IsTrue(this.se.IsExtensionSupported("http://someextension"));
		}
	}
}
