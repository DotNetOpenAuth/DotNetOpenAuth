using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Extensions.ProviderAuthenticationPolicy;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class PapeTests : ExtensionTestBase {
		[Test]
		public void None() {
			var response = ParameterizedTest<PolicyResponse>(
				TestSupport.Scenarios.ExtensionFullCooperation, Version, null);
			Assert.IsNull(response);
		}

		[Test]
		public void Full() {
			var request = new PolicyRequest();
			request.MaximumAuthenticationAge = TimeSpan.FromMinutes(10);
			var response = ParameterizedTest<PolicyResponse>(
				TestSupport.Scenarios.ExtensionFullCooperation, Version, request);
			Assert.IsNotNull(response);
			Assert.IsNotNull(response.AuthenticationTimeUtc);
			Assert.IsTrue(response.AuthenticationTimeUtc.Value > DateTime.UtcNow - request.MaximumAuthenticationAge);
		}
	}
}
