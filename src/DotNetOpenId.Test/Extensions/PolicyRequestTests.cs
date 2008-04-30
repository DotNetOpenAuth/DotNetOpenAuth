using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Extensions.ProviderAuthenticationPolicy;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class PolicyRequestTests {
		[Test]
		public void Ctor() {
			PolicyRequest req = new PolicyRequest();
			Assert.IsNull(req.MaximumAuthenticationAge);
			Assert.IsNotNull(req.PreferredPolicies);
			Assert.AreEqual(0, req.PreferredPolicies.Count);
		}

		[Test]
		public void MaximumAuthenticationAgeTest() {
			PolicyRequest req = new PolicyRequest();
			req.MaximumAuthenticationAge = TimeSpan.FromHours(1);
			Assert.IsNotNull(req.MaximumAuthenticationAge);
			Assert.AreEqual(TimeSpan.FromHours(1), req.MaximumAuthenticationAge);
			req.MaximumAuthenticationAge = null;
			Assert.IsNull(req.MaximumAuthenticationAge);
		}

		[Test]
		public void AddPolicies() {
			PolicyRequest resp = new PolicyRequest();
			resp.PreferredPolicies.Add(AuthenticationPolicies.MultiFactor);
			resp.PreferredPolicies.Add(AuthenticationPolicies.PhishingResistant);
			Assert.AreEqual(2, resp.PreferredPolicies.Count);
			Assert.AreEqual(AuthenticationPolicies.MultiFactor, resp.PreferredPolicies[0]);
			Assert.AreEqual(AuthenticationPolicies.PhishingResistant, resp.PreferredPolicies[1]);
		}

		[Test]
		public void AddPolicyMultipleTimes() {
			// Although this isn't really the desired behavior (we'd prefer to see an
			// exception thrown), since we're using a List<string> internally we can't
			// expect anything better (for now).  But if this is ever fixed, by all means
			// change this test to expect an exception or something else.
			PolicyRequest resp = new PolicyRequest();
			resp.PreferredPolicies.Add(AuthenticationPolicies.MultiFactor);
			resp.PreferredPolicies.Add(AuthenticationPolicies.MultiFactor);
			Assert.AreEqual(2, resp.PreferredPolicies.Count);
		}

		[Test]
		public void EqualsTest() {
			PolicyRequest req = new PolicyRequest();
			PolicyRequest req2 = new PolicyRequest();
			Assert.AreEqual(req, req2);
			Assert.AreNotEqual(req, null);
			Assert.AreNotEqual(null, req);

			// Test PreferredPolicies list comparison
			req.PreferredPolicies.Add(AuthenticationPolicies.PhishingResistant);
			Assert.AreNotEqual(req, req2);
			req2.PreferredPolicies.Add(AuthenticationPolicies.MultiFactor);
			Assert.AreNotEqual(req, req2);
			req2.PreferredPolicies.Clear();
			req2.PreferredPolicies.Add(AuthenticationPolicies.PhishingResistant);
			Assert.AreEqual(req, req2);

			// Test PreferredPolicies list comparison when that list is not in the same order.
			req.PreferredPolicies.Add(AuthenticationPolicies.MultiFactor);
			Assert.AreNotEqual(req, req2);
			req2.PreferredPolicies.Insert(0, AuthenticationPolicies.MultiFactor);
			Assert.AreEqual(req, req2);

			// Test MaximumAuthenticationAge comparison.
			req.MaximumAuthenticationAge = TimeSpan.FromHours(1);
			Assert.AreNotEqual(req, req2);
			req2.MaximumAuthenticationAge = req.MaximumAuthenticationAge;
			Assert.AreEqual(req, req2);
		}

		[Test]
		public void DeserializeNull() {
			PolicyRequest req = new PolicyRequest();
			Assert.IsFalse(((IExtensionRequest)req).Deserialize(null, null));
		}

		[Test]
		public void DeserializeEmpty() {
			PolicyRequest req = new PolicyRequest();
			Assert.IsFalse(((IExtensionRequest)req).Deserialize(new Dictionary<string, string>(), null));
		}

		[Test]
		public void SerializeRoundTrip() {
			// This test relies on the PolicyRequest.Equals method.  If this and that test 
			// are failing, work on EqualsTest first.

			// Most basic test
			PolicyRequest req = new PolicyRequest(), req2 = new PolicyRequest();
			var fields = ((IExtensionRequest)req).Serialize(null);
			Assert.IsTrue(((IExtensionRequest)req2).Deserialize(fields, null));
			Assert.AreEqual(req, req2);

			// Test with all fields set
			req2 = new PolicyRequest();
			req.PreferredPolicies.Add(AuthenticationPolicies.MultiFactor);
			req.MaximumAuthenticationAge = TimeSpan.FromHours(1);
			fields = ((IExtensionRequest)req).Serialize(null);
			Assert.IsTrue(((IExtensionRequest)req2).Deserialize(fields, null));
			Assert.AreEqual(req, req2);

			// Test with an extra policy
			req2 = new PolicyRequest();
			req.PreferredPolicies.Add(AuthenticationPolicies.PhishingResistant);
			fields = ((IExtensionRequest)req).Serialize(null);
			Assert.IsTrue(((IExtensionRequest)req2).Deserialize(fields, null));
			Assert.AreEqual(req, req2);

			// Test with a policy added twice.  We should see it intelligently leave one of
			// the doubled policies out.
			req2 = new PolicyRequest();
			req.PreferredPolicies.Add(AuthenticationPolicies.PhishingResistant);
			fields = ((IExtensionRequest)req).Serialize(null);
			Assert.IsTrue(((IExtensionRequest)req2).Deserialize(fields, null));
			Assert.AreNotEqual(req, req2);
			// Now go ahead and add the doubled one so we can do our equality test.
			req2.PreferredPolicies.Add(AuthenticationPolicies.PhishingResistant);
			Assert.AreEqual(req, req2);

		}
	}
}
