//-----------------------------------------------------------------------
// <copyright file="ProviderEndpointDescriptionTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using NUnit.Framework;

	[TestFixture]
	public class ProviderEndpointDescriptionTests : OpenIdTestBase {
		[TestCase]
		public void NonNullCapabilities() {
			var epd = new ProviderEndpointDescription(OPUri, Protocol.Default.Version);
			Assert.IsNotNull(epd.Capabilities);
		}

		[TestCase, ExpectedException(typeof(ProtocolException))]
		public void ProtocolDetectionWithoutClues() {
			new ProviderEndpointDescription(OPUri, new[] { Protocol.V20.HtmlDiscoveryLocalIdKey }); // random type URI irrelevant to detection
		}
	}
}
