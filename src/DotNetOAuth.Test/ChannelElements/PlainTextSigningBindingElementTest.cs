//-----------------------------------------------------------------------
// <copyright file="PlainTextSigningBindingElementTest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.ChannelElements
{
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messages;
	using DotNetOAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class PlainTextSigningBindingElementTest {
		[TestMethod]
		public void GetSignatureTest() {
			SigningBindingElementBase target = new PlainTextSigningBindingElement();
			ServiceProviderEndpoint endpoint = new ServiceProviderEndpoint("https://localtest", HttpDeliveryMethod.GetRequest);
			ITamperResistantOAuthMessage message = new RequestTokenMessage(endpoint);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			Assert.IsTrue(target.PrepareMessageForSending(message));
			Assert.AreEqual("PLAINTEXT", message.SignatureMethod);
			Assert.AreEqual("cs%26ts", message.Signature);
		}

		[TestMethod]
		public void GetNonEncryptedSignature() {
			SigningBindingElementBase target = new PlainTextSigningBindingElement();
			ServiceProviderEndpoint endpoint = new ServiceProviderEndpoint("http://localtest", HttpDeliveryMethod.GetRequest);
			ITamperResistantOAuthMessage message = new RequestTokenMessage(endpoint);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";

			// Since this is (non-encrypted) HTTP, so the plain text signer should not be used
			Assert.IsFalse(target.PrepareMessageForSending(message));
			Assert.IsNull(message.SignatureMethod);
			Assert.IsNull(message.Signature);
		}
	}
}
