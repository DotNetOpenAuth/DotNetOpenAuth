//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class OpenIdRelyingPartyTests : OpenIdTestBase {
		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
		}

		[TestMethod, Ignore] // ignored, pending work to make dumb mode a supported scenario.
		public void CtorNullAssociationStore() {
			new OpenIdRelyingParty(null, null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SecuritySettingsSetNull() {
			var rp = new OpenIdRelyingParty(new AssociationMemoryStore<Uri>(), new NonceMemoryStore(TimeSpan.FromMinutes(5)));
			rp.SecuritySettings = null;
		}
	}
}
