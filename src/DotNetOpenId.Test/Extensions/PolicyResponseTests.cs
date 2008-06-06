using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Extensions.ProviderAuthenticationPolicy;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class PolicyResponseTests {
		DateTime someLocalTime = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Local);
		DateTime someUtcTime = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc);
		DateTime someUnspecifiedTime = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Unspecified);
		[Test]
		public void Ctor() {
			PolicyResponse resp = new PolicyResponse();
			Assert.IsNotNull(resp.ActualPolicies);
			Assert.AreEqual(0, resp.ActualPolicies.Count);
			Assert.IsNull(resp.AuthenticationTimeUtc);
			Assert.IsNull(resp.NistAssuranceLevel);
		}

		[Test]
		public void AddPolicies() {
			PolicyResponse resp = new PolicyResponse();
			resp.ActualPolicies.Add(AuthenticationPolicies.MultiFactor);
			resp.ActualPolicies.Add(AuthenticationPolicies.PhishingResistant);
			Assert.AreEqual(2, resp.ActualPolicies.Count);
			Assert.AreEqual(AuthenticationPolicies.MultiFactor, resp.ActualPolicies[0]);
			Assert.AreEqual(AuthenticationPolicies.PhishingResistant, resp.ActualPolicies[1]);
		}

		[Test]
		public void AddPolicyMultipleTimes() {
			// Although this isn't really the desired behavior (we'd prefer to see an
			// exception thrown), since we're using a List<string> internally we can't
			// expect anything better (for now).  But if this is ever fixed, by all means
			// change this test to expect an exception or something else.
			PolicyResponse resp = new PolicyResponse();
			resp.ActualPolicies.Add(AuthenticationPolicies.MultiFactor);
			resp.ActualPolicies.Add(AuthenticationPolicies.MultiFactor);
			Assert.AreEqual(2, resp.ActualPolicies.Count);
		}

		[Test]
		public void AuthenticationTimeUtcConvertsToUtc() {
			PolicyResponse resp = new PolicyResponse();
			resp.AuthenticationTimeUtc = someLocalTime;
			Assert.IsNotNull(resp.AuthenticationTimeUtc);
			Assert.AreEqual(DateTimeKind.Utc, resp.AuthenticationTimeUtc.Value.Kind);
			Assert.AreEqual(someLocalTime.ToUniversalTime(), resp.AuthenticationTimeUtc.Value);
		}

		[Test]
		public void AuthenticationTimeUtcSetUtc() {
			PolicyResponse resp = new PolicyResponse();
			resp.AuthenticationTimeUtc = someUtcTime;
			Assert.AreEqual(someUtcTime, resp.AuthenticationTimeUtc);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void AuthenticationTimeUtcSetUnspecified() {
			PolicyResponse resp = new PolicyResponse();
			resp.AuthenticationTimeUtc = someUnspecifiedTime;
		}

		[Test]
		public void AuthenticationTimeUtcSetNull() {
			PolicyResponse resp = new PolicyResponse();
			resp.AuthenticationTimeUtc = null;
			Assert.IsNull(resp.AuthenticationTimeUtc);
			resp.AuthenticationTimeUtc = someUtcTime;
			Assert.IsNotNull(resp.AuthenticationTimeUtc);
			resp.AuthenticationTimeUtc = null;
			Assert.IsNull(resp.AuthenticationTimeUtc);
		}

		[Test]
		public void NistAssuranceLevelSetVarious() {
			PolicyResponse resp = new PolicyResponse();
			resp.NistAssuranceLevel = NistAssuranceLevel.Level1;
			Assert.AreEqual(NistAssuranceLevel.Level1, resp.NistAssuranceLevel);
			resp.NistAssuranceLevel = null;
			Assert.IsNull(resp.NistAssuranceLevel);
			resp.NistAssuranceLevel = NistAssuranceLevel.InsufficientForLevel1;
			Assert.AreEqual(NistAssuranceLevel.InsufficientForLevel1, resp.NistAssuranceLevel);
		}

		[Test]
		public void EqualsTest() {
			PolicyResponse resp = new PolicyResponse();
			PolicyResponse resp2 = new PolicyResponse();
			Assert.AreEqual(resp, resp2);
			Assert.AreNotEqual(resp, null);
			Assert.AreNotEqual(null, resp);

			// Test ActualPolicies list comparison
			resp.ActualPolicies.Add(AuthenticationPolicies.PhishingResistant);
			Assert.AreNotEqual(resp, resp2);
			resp2.ActualPolicies.Add(AuthenticationPolicies.MultiFactor);
			Assert.AreNotEqual(resp, resp2);
			resp2.ActualPolicies.Clear();
			resp2.ActualPolicies.Add(AuthenticationPolicies.PhishingResistant);
			Assert.AreEqual(resp, resp2);

			// Test ActualPolicies list comparison when that list is not in the same order.
			resp.ActualPolicies.Add(AuthenticationPolicies.MultiFactor);
			Assert.AreNotEqual(resp, resp2);
			resp2.ActualPolicies.Insert(0, AuthenticationPolicies.MultiFactor);
			Assert.AreEqual(resp, resp2);

			// Test AuthenticationTimeUtc comparison.
			resp.AuthenticationTimeUtc = DateTime.Now;
			Assert.AreNotEqual(resp, resp2);
			resp2.AuthenticationTimeUtc = resp.AuthenticationTimeUtc;
			Assert.AreEqual(resp, resp2);
			resp2.AuthenticationTimeUtc += TimeSpan.FromSeconds(1);
			Assert.AreNotEqual(resp, resp2);
			resp2.AuthenticationTimeUtc = resp.AuthenticationTimeUtc;
			Assert.AreEqual(resp, resp2);

			// Test NistAssuranceLevel comparison.
			resp.NistAssuranceLevel = NistAssuranceLevel.InsufficientForLevel1;
			Assert.AreNotEqual(resp, resp2);
			resp2.NistAssuranceLevel = NistAssuranceLevel.InsufficientForLevel1;
			Assert.AreEqual(resp, resp2);
			resp.NistAssuranceLevel = NistAssuranceLevel.Level2;
			Assert.AreNotEqual(resp, resp2);
			resp2.NistAssuranceLevel = NistAssuranceLevel.Level2;
			Assert.AreEqual(resp, resp2);
		}

		[Test]
		public void DeserializeNull() {
			PolicyResponse resp = new PolicyResponse();
			Assert.IsFalse(((IExtensionResponse)resp).Deserialize(null, null, Constants.TypeUri));
		}

		[Test]
		public void DeserializeEmpty() {
			PolicyResponse resp = new PolicyResponse();
			Assert.IsFalse(((IExtensionResponse)resp).Deserialize(new Dictionary<string, string>(), null, Constants.TypeUri));
		}

		[Test]
		public void SerializeRoundTrip() {
			// This test relies on the PolicyResponse.Equals method.  If this and that test 
			// are failing, work on EqualsTest first.

			// Most basic test
			PolicyResponse resp = new PolicyResponse(), resp2 = new PolicyResponse();
			var fields = ((IExtensionResponse)resp).Serialize(null);
			Assert.IsTrue(((IExtensionResponse)resp2).Deserialize(fields, null, Constants.TypeUri));
			Assert.AreEqual(resp, resp2);

			// Test with all fields set
			resp2 = new PolicyResponse();
			resp.ActualPolicies.Add(AuthenticationPolicies.MultiFactor);
			resp.AuthenticationTimeUtc = someUtcTime;
			resp.NistAssuranceLevel = NistAssuranceLevel.Level2;
			fields = ((IExtensionResponse)resp).Serialize(null);
			Assert.IsTrue(((IExtensionResponse)resp2).Deserialize(fields, null, Constants.TypeUri));
			Assert.AreEqual(resp, resp2);

			// Test with an extra policy
			resp2 = new PolicyResponse();
			resp.ActualPolicies.Add(AuthenticationPolicies.PhishingResistant);
			fields = ((IExtensionResponse)resp).Serialize(null);
			Assert.IsTrue(((IExtensionResponse)resp2).Deserialize(fields, null, Constants.TypeUri));
			Assert.AreEqual(resp, resp2);

			// Test with a policy added twice.  We should see it intelligently leave one of
			// the doubled policies out.
			resp2 = new PolicyResponse();
			resp.ActualPolicies.Add(AuthenticationPolicies.PhishingResistant);
			fields = ((IExtensionResponse)resp).Serialize(null);
			Assert.IsTrue(((IExtensionResponse)resp2).Deserialize(fields, null, Constants.TypeUri));
			Assert.AreNotEqual(resp, resp2);
			// Now go ahead and add the doubled one so we can do our equality test.
			resp2.ActualPolicies.Add(AuthenticationPolicies.PhishingResistant);
			Assert.AreEqual(resp, resp2);
		}
	}
}
