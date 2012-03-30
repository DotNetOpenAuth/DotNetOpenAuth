//-----------------------------------------------------------------------
// <copyright file="SectionTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Configuration {
	using System;
	using System.Linq;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.OpenId;
	using NUnit.Framework;

	[TestFixture]
	public class SectionTests {
		[Test]
		public void UntrustedWebRequest() {
			var uwr = DotNetOpenAuthSection.Messaging.UntrustedWebRequest;

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

		[Test]
		public void OpenIdMaxAuthenticationTime() {
			Assert.AreEqual(TimeSpan.Parse("00:08:17"), OpenIdElement.Configuration.MaxAuthenticationTime);
		}

		[Test]
		public void OpenIdRelyingParty() {
			var rp = OpenIdElement.Configuration.RelyingParty;
			Assert.IsNull(rp.ApplicationStore.CustomType);

			Assert.AreEqual(ProtocolVersion.V10, rp.SecuritySettings.MinimumRequiredOpenIdVersion);
			Assert.AreEqual(6, rp.SecuritySettings.MinimumHashBitLength);
			Assert.AreEqual(301, rp.SecuritySettings.MaximumHashBitLength);
			Assert.IsFalse(rp.SecuritySettings.RequireSsl);
		}

		[Test]
		public void OpenIdProvider() {
			var op = OpenIdElement.Configuration.Provider;
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
