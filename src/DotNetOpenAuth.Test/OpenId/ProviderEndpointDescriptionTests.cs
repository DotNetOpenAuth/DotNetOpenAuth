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
		[TestMethod]
		public void NonNullCapabilities() {
			var epd = new ProviderEndpointDescription(OPUri, Protocol.Default.Version);
			Assert.IsNotNull(epd.Capabilities);
		}
	}
}
