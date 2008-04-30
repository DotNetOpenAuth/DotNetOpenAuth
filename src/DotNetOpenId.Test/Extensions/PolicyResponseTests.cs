using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Extensions.AuthenticationPolicy;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class PolicyResponseTests {
		[Test]
		public void Ctor() {
			PolicyResponse resp = new PolicyResponse();
			Assert.AreEqual(0, resp.ActualPolicies.Count);
			Assert.IsNull(resp.AuthenticationTimeUtc);
			Assert.IsNull(resp.NistAssuranceLevel);
		}
	}
}
