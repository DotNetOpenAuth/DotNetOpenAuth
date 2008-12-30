//-----------------------------------------------------------------------
// <copyright file="ExtensionsBindingElementTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using DotNetOpenAuth.Test.OpenId.Extensions;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ExtensionsBindingElementTests : OpenIdTestBase {
		private OpenIdExtensionFactory factory;
		private ExtensionsBindingElement element;
		private IProtocolMessageWithExtensions request;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.factory = new OpenIdExtensionFactory();
			this.factory.RegisterExtension(MockOpenIdExtension.Factory);
			this.element = new ExtensionsBindingElement(this.factory);
			this.request = new SignedResponseRequest(Protocol.Default.Version, OpenIdTestBase.ProviderUri, AuthenticationRequestMode.Immediate);
		}

		[TestMethod]
		public void RoundTripFullStackTest() {
			IOpenIdMessageExtension request = new MockOpenIdExtension("requestPart", "requestData");
			IOpenIdMessageExtension response = new MockOpenIdExtension("responsePart", "responseData");
			ExtensionTestUtilities.Roundtrip(
				Protocol.Default,
				new IOpenIdMessageExtension[] { request },
				new IOpenIdMessageExtension[] { response });
		}

		[TestMethod]
		public void ExtensionFactory() {
			Assert.AreSame(this.factory, this.element.ExtensionFactory);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void PrepareMessageForSendingNull() {
			this.element.PrepareMessageForSending(null);
		}

		/// <summary>
		/// Verifies that false is returned when a non-extendable message is sent.
		/// </summary>
		[TestMethod]
		public void PrepareMessageForSendingNonExtendableMessage() {
			IProtocolMessage request = new AssociateDiffieHellmanRequest(Protocol.Default.Version, OpenIdTestBase.ProviderUri);
			Assert.IsFalse(this.element.PrepareMessageForSending(request));
		}

		[TestMethod]
		public void PrepareMessageForSending() {
			this.request.Extensions.Add(new MockOpenIdExtension("part", "extra"));
			Assert.IsTrue(this.element.PrepareMessageForSending(this.request));

			string alias = GetAliases(this.request.ExtraData).Single();
			Assert.AreEqual(MockOpenIdExtension.MockTypeUri, this.request.ExtraData["openid.ns." + alias]);
			Assert.AreEqual("part", this.request.ExtraData["openid." + alias + ".Part"]);
			Assert.AreEqual("extra", this.request.ExtraData["openid." + alias + ".data"]);
		}

		[TestMethod]
		public void PrepareMessageForReceiving() {
			this.request.ExtraData["openid.ns.mock"] = MockOpenIdExtension.MockTypeUri;
			this.request.ExtraData["openid.mock.Part"] = "part";
			this.request.ExtraData["openid.mock.data"] = "extra";
			Assert.IsTrue(this.element.PrepareMessageForReceiving(this.request));
			MockOpenIdExtension ext = this.request.Extensions.OfType<MockOpenIdExtension>().Single();
			Assert.AreEqual("part", ext.Part);
			Assert.AreEqual("extra", ext.Data);
		}

		/// <summary>
		/// Verifies that unsigned extension responses (where any or all fields are unsigned) are ignored.
		/// </summary>
		[TestMethod, Ignore]
		public void UnsignedExtensionsAreIgnored() {
			Assert.Inconclusive("Not yet implemented.");
		}

		private static IEnumerable<string> GetAliases(IDictionary<string, string> extraData) {
			Regex regex = new Regex(@"^openid\.ns\.(\w+)");
			return from key in extraData.Keys
				   let m = regex.Match(key)
				   where m.Success
				   select m.Groups[1].Value;
		}
	}
}
