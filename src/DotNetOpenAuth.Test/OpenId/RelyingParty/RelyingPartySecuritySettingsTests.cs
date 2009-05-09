//-----------------------------------------------------------------------
// <copyright file="RelyingPartySecuritySettingsTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class RelyingPartySecuritySettingsTests : OpenIdTestBase {
		private RelyingPartySecuritySettings settings;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.settings = new RelyingPartySecuritySettings();
		}

		/// <summary>
		/// Verifies that the <see cref="RelyingPartySecuritySettings.RequireSsl"/> property
		/// getter/setter are implemented correctly.
		/// </summary>
		[TestMethod]
		public void RequireSsl() {
			Assert.IsFalse(this.settings.RequireSsl, "Default should be to not require SSL.");
			this.settings.RequireSsl = true;
			Assert.IsTrue(this.settings.RequireSsl);
			this.settings.RequireSsl = false;
			Assert.IsFalse(this.settings.RequireSsl);
		}
	}
}
