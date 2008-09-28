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
		public void HttpsSignatureGeneration() {
			SigningBindingElementBase target = new PlainTextSigningBindingElement();
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("https://localtest", HttpDeliveryMethod.GetRequest);
			ITamperResistantOAuthMessage message = new RequestTokenMessage(endpoint);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			Assert.IsTrue(target.PrepareMessageForSending(message));
			Assert.AreEqual("PLAINTEXT", message.SignatureMethod);
			Assert.AreEqual("cs%26ts", message.Signature);
		}

		[TestMethod]
		public void HttpsSignatureVerification() {
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("https://localtest", HttpDeliveryMethod.GetRequest);
			ITamperProtectionChannelBindingElement target = new PlainTextSigningBindingElement();
			ITamperResistantOAuthMessage message = new RequestTokenMessage(endpoint);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			message.SignatureMethod = "PLAINTEXT";
			message.Signature = "cs%26ts";
			Assert.IsTrue(target.PrepareMessageForReceiving(message));
		}

		[TestMethod]
		public void HttpsSignatureVerificationNotApplicable() {
			SigningBindingElementBase target = new PlainTextSigningBindingElement();
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("https://localtest", HttpDeliveryMethod.GetRequest);
			ITamperResistantOAuthMessage message = new RequestTokenMessage(endpoint);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			message.SignatureMethod = "ANOTHERALGORITHM";
			message.Signature = "somethingelse";
			Assert.IsFalse(target.PrepareMessageForReceiving(message), "PLAINTEXT binding element should opt-out where it doesn't understand.");
		}

		[TestMethod]
		public void HttpSignatureGeneration() {
			SigningBindingElementBase target = new PlainTextSigningBindingElement();
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("http://localtest", HttpDeliveryMethod.GetRequest);
			ITamperResistantOAuthMessage message = new RequestTokenMessage(endpoint);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";

			// Since this is (non-encrypted) HTTP, so the plain text signer should not be used
			Assert.IsFalse(target.PrepareMessageForSending(message));
			Assert.IsNull(message.SignatureMethod);
			Assert.IsNull(message.Signature);
		}

		[TestMethod]
		public void HttpSignatureVerification() {
			SigningBindingElementBase target = new PlainTextSigningBindingElement();
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("http://localtest", HttpDeliveryMethod.GetRequest);
			ITamperResistantOAuthMessage message = new RequestTokenMessage(endpoint);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			message.SignatureMethod = "PLAINTEXT";
			message.Signature = "cs%26ts";
			Assert.IsFalse(target.PrepareMessageForReceiving(message), "PLAINTEXT signature binding element should refuse to participate in non-encrypted messages.");
		}
	}
}
