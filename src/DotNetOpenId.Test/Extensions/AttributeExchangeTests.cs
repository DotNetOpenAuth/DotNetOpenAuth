using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class AttributeExchangeTests : ExtensionTestBase {
		[Test]
		public void None() {
			var fetchResponse = ParameterizedTest<AttributeExchangeFetchResponse>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), null);
			Assert.IsNull(fetchResponse);
			var storeResponse = ParameterizedTest<AttributeExchangeStoreResponse>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), null);
			Assert.IsNull(storeResponse);
		}

		[Test]
		public void Fetch() {
			var request = new AttributeExchangeFetchRequest();
			var response = ParameterizedTest<AttributeExchangeFetchResponse>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), request);
			Assert.IsNotNull(response);
		}

		[Test]
		public void Store() {
			var request = new AttributeExchangeStoreRequest();
			var response = ParameterizedTest<AttributeExchangeStoreResponse>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), request);
			Assert.IsNotNull(response);
		}
	}
}
