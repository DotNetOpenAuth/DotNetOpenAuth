//-----------------------------------------------------------------------
// <copyright file="SectionTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Configuration {
	using System;
	using System.Linq;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.OpenId;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class SectionTests {
		[TestMethod]
		public void UntrustedWebRequest() {
			var uwr = DotNetOpenAuthSection.Configuration.Messaging.UntrustedWebRequest;

			Assert.AreEqual(TimeSpan.Parse("01:23:45"), uwr.Timeout);
			Assert.AreEqual(TimeSpan.Parse("01:23:56"), uwr.ReadWriteTimeout);
			Assert.AreEqual(500001, uwr.MaximumBytesToRead);
			Assert.AreEqual(9, uwr.MaximumRedirections);

			// Verify whitelists and blacklists
			Assert.IsTrue(uwr.BlacklistHosts.KeysAsStrings.Contains("positivelyevil"));
			Assert.IsTrue(uwr.BlacklistHostsRegex.KeysAsStrings.Contains(".+veryevil.+"));
			Assert.IsTrue(uwr.WhitelistHosts.KeysAsStrings.Contains("evilButTrusted"));
			Assert.IsTrue(uwr.WhitelistHostsRegex.KeysAsStrings.Contains(".+trusted.+"));
		}

		[TestMethod]
		public void OpenIdMaxAuthenticationTime() {
			Assert.AreEqual(TimeSpan.Parse("00:08:17"), DotNetOpenAuthSection.Configuration.OpenId.MaxAuthenticationTime);
		}

		[TestMethod]
		public void OpenIdRelyingParty() {
			var rp = DotNetOpenAuthSection.Configuration.OpenId.RelyingParty;
			Assert.IsNull(rp.ApplicationStore.CustomType);

			Assert.AreEqual(ProtocolVersion.V10, rp.SecuritySettings.MinimumRequiredOpenIdVersion);
			Assert.AreEqual(6, rp.SecuritySettings.MinimumHashBitLength);
			Assert.AreEqual(301, rp.SecuritySettings.MaximumHashBitLength);
			Assert.IsFalse(rp.SecuritySettings.RequireSsl);
		}

		[TestMethod]
		public void OpenIdProvider() {
			var op = DotNetOpenAuthSection.Configuration.OpenId.Provider;
			Assert.IsNull(op.ApplicationStore.CustomType);

			Assert.IsTrue(op.SecuritySettings.ProtectDownlevelReplayAttacks);
			Assert.AreEqual(7, op.SecuritySettings.MinimumHashBitLength);
			Assert.AreEqual(302, op.SecuritySettings.MaximumHashBitLength);

			Assert.AreEqual(2, op.SecuritySettings.AssociationLifetimes.Count);
			Assert.AreEqual(TimeSpan.Parse("2.00:00:02"), op.SecuritySettings.AssociationLifetimes.Single(a => a.AssociationType == "HMAC-SHA1").MaximumLifetime);
			Assert.AreEqual(TimeSpan.Parse("14.00:00:14"), op.SecuritySettings.AssociationLifetimes.Single(a => a.AssociationType == "HMAC-SHA256").MaximumLifetime);
		}
	}
}
