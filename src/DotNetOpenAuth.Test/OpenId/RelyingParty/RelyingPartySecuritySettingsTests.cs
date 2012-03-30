//-----------------------------------------------------------------------
// <copyright file="RelyingPartySecuritySettingsTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class RelyingPartySecuritySettingsTests : OpenIdTestBase {
		private RelyingPartySecuritySettings settings;

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			this.settings = new RelyingPartySecuritySettings();
		}

		[Test]
		public void Defaults() {
			Assert.IsFalse(this.settings.RejectUnsolicitedAssertions);
			Assert.IsFalse(this.settings.RequireSsl, "Default should be to not require SSL.");
		}

		/// <summary>
		/// Verifies that the <see cref="RelyingPartySecuritySettings.RequireSsl"/> property
		/// getter/setter are implemented correctly.
		/// </summary>
		[Test]
		public void RequireSsl() {
			this.settings.RequireSsl = true;
			Assert.IsTrue(this.settings.RequireSsl);
			this.settings.RequireSsl = false;
			Assert.IsFalse(this.settings.RequireSsl);
		}

		/// <summary>
		/// Verifies that the <see cref="RelyingPartySecuritySettings.RequireDirectedIdentity"/>
		/// property getter/setter are implemented correctly.
		/// </summary>
		[Test]
		public void RequireDirectedIdentity() {
			this.settings.RequireDirectedIdentity = true;
			Assert.IsTrue(this.settings.RequireDirectedIdentity);
			this.settings.RequireDirectedIdentity = false;
			Assert.IsFalse(this.settings.RequireDirectedIdentity);
		}

		/// <summary>
		/// Verifies that the <see cref="RelyingPartySecuritySettings.RequireAssociation"/>
		/// property getter/setter are implemented correctly.
		/// </summary>
		[Test]
		public void RequireAssociation() {
			this.settings.RequireAssociation = true;
			Assert.IsTrue(this.settings.RequireAssociation);
			this.settings.RequireAssociation = false;
			Assert.IsFalse(this.settings.RequireAssociation);
		}
	}
}
