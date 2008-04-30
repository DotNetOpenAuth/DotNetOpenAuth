using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Extensions.AuthenticationPolicy;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class PolicyRequestTests {
		[Test]
		public void Ctor() {
			PolicyRequest req = new PolicyRequest();
			Assert.AreEqual(0, req.PreferredPolicies.Count);
			Assert.IsNull(req.MaximumAuthenticationAge);
		}
	}
}
