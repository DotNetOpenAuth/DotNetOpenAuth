//-----------------------------------------------------------------------
// <copyright file="PolicyRequestTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions.ProviderAuthenticationPolicy {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using NUnit.Framework;

	[TestFixture]
	public class PolicyRequestTests : OpenIdTestBase {
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
		public void AddAuthLevelTypes() {
			PolicyRequest req = new PolicyRequest();
			req.PreferredAuthLevelTypes.Add(Constants.AssuranceLevels.NistTypeUri);
			Assert.AreEqual(1, req.PreferredAuthLevelTypes.Count);
			Assert.IsTrue(req.PreferredAuthLevelTypes.Contains(Constants.AssuranceLevels.NistTypeUri));
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

			// Test PreferredAuthLevelTypes comparison.
			req.PreferredAuthLevelTypes.Add("authlevel1");
			Assert.AreNotEqual(req, req2);
			req2.PreferredAuthLevelTypes.Add("authlevel2");
			Assert.AreNotEqual(req, req2);
			req.PreferredAuthLevelTypes.Add("authlevel2");
			req2.PreferredAuthLevelTypes.Add("authlevel1");
			Assert.AreEqual(req, req2);
		}

		[Test]
		public void Serialize() {
			PolicyRequest req = new PolicyRequest();
			IMessageWithEvents reqEvents = req;

			var fields = this.MessageDescriptions.GetAccessor(req);
			reqEvents.OnSending();
			Assert.AreEqual(1, fields.Count);
			Assert.IsTrue(fields.ContainsKey("preferred_auth_policies"));
			Assert.AreEqual(string.Empty, fields["preferred_auth_policies"]);

			req.MaximumAuthenticationAge = TimeSpan.FromHours(1);
			reqEvents.OnSending();
			Assert.AreEqual(2, fields.Count);
			Assert.IsTrue(fields.ContainsKey("max_auth_age"));
			Assert.AreEqual(TimeSpan.FromHours(1).TotalSeconds.ToString(CultureInfo.InvariantCulture), fields["max_auth_age"]);

			req.PreferredPolicies.Add("http://pol1/");
			reqEvents.OnSending();
			Assert.AreEqual("http://pol1/", fields["preferred_auth_policies"]);

			req.PreferredPolicies.Add("http://pol2/");
			reqEvents.OnSending();
			Assert.AreEqual("http://pol1/ http://pol2/", fields["preferred_auth_policies"]);

			req.PreferredAuthLevelTypes.Add("http://authtype1/");
			reqEvents.OnSending();
			Assert.AreEqual(4, fields.Count);
			Assert.IsTrue(fields.ContainsKey("auth_level.ns.alias1"));
			Assert.AreEqual("http://authtype1/", fields["auth_level.ns.alias1"]);
			Assert.IsTrue(fields.ContainsKey("preferred_auth_level_types"));
			Assert.AreEqual("alias1", fields["preferred_auth_level_types"]);

			req.PreferredAuthLevelTypes.Add(Constants.AssuranceLevels.NistTypeUri);
			reqEvents.OnSending();
			Assert.AreEqual(5, fields.Count);
			Assert.IsTrue(fields.ContainsKey("auth_level.ns.alias2"));
			Assert.AreEqual("http://authtype1/", fields["auth_level.ns.alias2"]);
			Assert.IsTrue(fields.ContainsKey("auth_level.ns.nist"));
			Assert.AreEqual(Constants.AssuranceLevels.NistTypeUri, fields["auth_level.ns.nist"]);
			Assert.AreEqual("alias2 nist", fields["preferred_auth_level_types"]);
		}
	}
}
