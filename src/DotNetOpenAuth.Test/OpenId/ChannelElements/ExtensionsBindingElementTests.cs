//-----------------------------------------------------------------------
// <copyright file="ExtensionsBindingElementTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.Test.Mocks;
	using DotNetOpenAuth.Test.OpenId.Extensions;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions;

	[TestClass]
	public class ExtensionsBindingElementTests : OpenIdTestBase {
		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
		}

		[TestMethod]
		public void ExtensionTransportTest() {
			IOpenIdMessageExtension request = new MockOpenIdExtension("requestPart", "requestData");
			IOpenIdMessageExtension response = new MockOpenIdExtension("responsePart", "responseData");
			ExtensionTestUtilities.Roundtrip(
				Protocol.Default,
				new IOpenIdMessageExtension[] { request },
				new IOpenIdMessageExtension[] { response });
		}
	}
}
