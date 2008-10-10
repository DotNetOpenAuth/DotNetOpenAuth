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
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), null);
			Assert.IsNull(response);
		}

		[Test]
		public void Full() {
			var request = new PolicyRequest();
			request.MaximumAuthenticationAge = TimeSpan.FromMinutes(10);
			request.PreferredAuthLevelTypes.Add(Constants.AuthenticationLevels.NistTypeUri);
			var response = ParameterizedTest<PolicyResponse>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), request);
			Assert.IsNotNull(response);
			Assert.IsNotNull(response.AuthenticationTimeUtc);
			Assert.IsTrue(response.AuthenticationTimeUtc.Value > DateTime.UtcNow - request.MaximumAuthenticationAge);
			Assert.IsTrue(response.AssuranceLevels.ContainsKey(Constants.AuthenticationLevels.NistTypeUri));
			Assert.AreEqual("1", response.AssuranceLevels[Constants.AuthenticationLevels.NistTypeUri]);
			Assert.AreEqual(NistAssuranceLevel.Level1, response.NistAssuranceLevel);
		}
	}
}
