//-----------------------------------------------------------------------
// <copyright file="PolicyResponseTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions.ProviderAuthenticationPolicy {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using NUnit.Framework;

	[TestFixture]
	public class PolicyResponseTests : OpenIdTestBase {
		private static readonly DateTime someLocalTime = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Local);
		private static readonly DateTime someUtcTime = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc);
		private static readonly DateTime someUnspecifiedTime = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Unspecified);

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
		public void AssuranceLevels() {
			PolicyResponse resp = new PolicyResponse();
			Assert.AreEqual(0, resp.AssuranceLevels.Count);
			resp.NistAssuranceLevel = NistAssuranceLevel.Level2;
			Assert.AreEqual(1, resp.AssuranceLevels.Count);
			Assert.AreEqual("2", resp.AssuranceLevels[Constants.AssuranceLevels.NistTypeUri]);
			resp.AssuranceLevels[Constants.AssuranceLevels.NistTypeUri] = "3";
			Assert.AreEqual(NistAssuranceLevel.Level3, resp.NistAssuranceLevel);
			resp.AssuranceLevels.Clear();
			Assert.IsNull(resp.NistAssuranceLevel);
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

			// Test AssuranceLevels comparison.
			resp.AssuranceLevels.Add("custom", "b");
			Assert.AreNotEqual(resp, resp2);
			resp2.AssuranceLevels.Add("custom", "2");
			Assert.AreNotEqual(resp, resp2);
			resp2.AssuranceLevels["custom"] = "b";
			Assert.AreEqual(resp, resp2);
			resp.AssuranceLevels[Constants.AssuranceLevels.NistTypeUri] = "1";
			Assert.AreNotEqual(resp, resp2);
			resp2.AssuranceLevels[Constants.AssuranceLevels.NistTypeUri] = "1";
			Assert.AreEqual(resp, resp2);
		}

		[Test]
		public void Serialize() {
			PolicyResponse resp = new PolicyResponse();
			IMessageWithEvents respEvents = resp;

			var fields = this.MessageDescriptions.GetAccessor(resp);
			respEvents.OnSending();
			Assert.AreEqual(1, fields.Count);
			Assert.IsTrue(fields.ContainsKey("auth_policies"));
			Assert.AreEqual(AuthenticationPolicies.None, fields["auth_policies"]);

			resp.ActualPolicies.Add(AuthenticationPolicies.PhishingResistant);
			respEvents.OnSending();
			Assert.AreEqual(1, fields.Count);
			Assert.AreEqual(AuthenticationPolicies.PhishingResistant, fields["auth_policies"]);

			resp.ActualPolicies.Add(AuthenticationPolicies.PhysicalMultiFactor);
			respEvents.OnSending();
			Assert.AreEqual(1, fields.Count);
			Assert.AreEqual(
				AuthenticationPolicies.PhishingResistant + " " + AuthenticationPolicies.PhysicalMultiFactor,
				fields["auth_policies"]);

			resp.AuthenticationTimeUtc = DateTime.UtcNow;
			respEvents.OnSending();
			Assert.AreEqual(2, fields.Count);
			Assert.IsTrue(fields.ContainsKey("auth_time"));

			resp.NistAssuranceLevel = NistAssuranceLevel.Level3;
			respEvents.OnSending();
			Assert.AreEqual(4, fields.Count);
			Assert.IsTrue(fields.ContainsKey("auth_level.ns.nist"));
			Assert.AreEqual(Constants.AssuranceLevels.NistTypeUri, fields["auth_level.ns.nist"]);
			Assert.IsTrue(fields.ContainsKey("auth_level.nist"));
			Assert.AreEqual("3", fields["auth_level.nist"]);

			resp.AssuranceLevels.Add("custom", "CU");
			respEvents.OnSending();
			Assert.AreEqual(6, fields.Count);
			Assert.IsTrue(fields.ContainsKey("auth_level.ns.alias2"));
			Assert.AreEqual("custom", fields["auth_level.ns.alias2"]);
			Assert.IsTrue(fields.ContainsKey("auth_level.alias2"));
			Assert.AreEqual("CU", fields["auth_level.alias2"]);
			// and make sure the NIST is still there.
			Assert.IsTrue(fields.ContainsKey("auth_level.ns.nist"));
			Assert.AreEqual(Constants.AssuranceLevels.NistTypeUri, fields["auth_level.ns.nist"]);
			Assert.IsTrue(fields.ContainsKey("auth_level.nist"));
			Assert.AreEqual("3", fields["auth_level.nist"]);
		}
	}
}
