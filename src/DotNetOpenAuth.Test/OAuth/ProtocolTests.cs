//-----------------------------------------------------------------------
// <copyright file="ProtocolTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth {
	using DotNetOpenAuth.OAuth;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ProtocolTests {
		[TestMethod]
		public void Default() {
			Assert.AreSame(Protocol.V10a, Protocol.Default);
		}

		[TestMethod]
		public void DataContractNamespace() {
			Assert.AreEqual("http://oauth.net/core/1.0/", Protocol.V10.DataContractNamespace);
			Assert.AreEqual("http://oauth.net/core/1.0/", Protocol.DataContractNamespaceV10);
		}

		[TestMethod]
		public void AuthorizationHeaderScheme() {
			Assert.AreEqual("OAuth", Protocol.AuthorizationHeaderScheme);
		}

		[TestMethod]
		public void ParameterPrefix() {
			Assert.AreEqual("oauth_", Protocol.ParameterPrefix);
		}
	}
}
