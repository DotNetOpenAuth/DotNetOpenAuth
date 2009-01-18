//-----------------------------------------------------------------------
// <copyright file="PapeTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Test.Extensions.ProviderAuthenticationPolicy {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;

	[TestClass]
	public class PapeTests : ExtensionTestBase {
		[TestMethod]
		public void None() {
			var response = ParameterizedTest<PolicyResponse>(
				TestSupport.Scenarios.ExtensionFullCooperation, Version, null);
			Assert.IsNull(response);
		}

		[TestMethod]
		public void Full() {
			var request = new PolicyRequest();
			request.MaximumAuthenticationAge = TimeSpan.FromMinutes(10);
			request.PreferredAuthLevelTypes.Add(Constants.AuthenticationLevels.NistTypeUri);
			var response = ParameterizedTest<PolicyResponse>(
				TestSupport.Scenarios.ExtensionFullCooperation, Version, request);
			Assert.IsNotNull(response);
			Assert.IsNotNull(response.AuthenticationTimeUtc);
			Assert.IsTrue(response.AuthenticationTimeUtc.Value > DateTime.UtcNow - request.MaximumAuthenticationAge);
			Assert.IsTrue(response.AssuranceLevels.ContainsKey(Constants.AuthenticationLevels.NistTypeUri));
			Assert.AreEqual("1", response.AssuranceLevels[Constants.AuthenticationLevels.NistTypeUri]);
			Assert.AreEqual(NistAssuranceLevel.Level1, response.NistAssuranceLevel);
		}
	}
}
