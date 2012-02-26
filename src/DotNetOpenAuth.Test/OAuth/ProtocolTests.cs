//-----------------------------------------------------------------------
// <copyright file="ProtocolTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth {
	using DotNetOpenAuth.OAuth;
	using NUnit.Framework;

	[TestFixture]
	public class ProtocolTests {
		[Test]
		public void Default() {
			Assert.AreSame(Protocol.V10a, Protocol.Default);
		}

		[Test]
		public void DataContractNamespace() {
			Assert.AreEqual("http://oauth.net/core/1.0/", Protocol.V10.DataContractNamespace);
			Assert.AreEqual("http://oauth.net/core/1.0/", Protocol.DataContractNamespaceV10);
		}

		[Test]
		public void AuthorizationHeaderScheme() {
			Assert.AreEqual("OAuth", Protocol.AuthorizationHeaderScheme);
		}

		[Test]
		public void ParameterPrefix() {
			Assert.AreEqual("oauth_", Protocol.ParameterPrefix);
		}
	}
}
