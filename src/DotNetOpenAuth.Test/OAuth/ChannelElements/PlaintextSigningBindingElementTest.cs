//-----------------------------------------------------------------------
// <copyright file="PlaintextSigningBindingElementTest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth.ChannelElements {
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class PlaintextSigningBindingElementTest {
		[Test]
		public void HttpsSignatureGeneration() {
			SigningBindingElementBase target = new PlaintextSigningBindingElement();
			target.Channel = new TestChannel();
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("https://localtest", HttpDeliveryMethods.GetRequest);
			ITamperResistantOAuthMessage message = new UnauthorizedTokenRequest(endpoint, Protocol.Default.Version);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			Assert.IsNotNull(target.ProcessOutgoingMessage(message));
			Assert.AreEqual("PLAINTEXT", message.SignatureMethod);
			Assert.AreEqual("cs&ts", message.Signature);
		}

		[Test]
		public void HttpsSignatureVerification() {
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("https://localtest", HttpDeliveryMethods.GetRequest);
			ITamperProtectionChannelBindingElement target = new PlaintextSigningBindingElement();
			target.Channel = new TestChannel();
			ITamperResistantOAuthMessage message = new UnauthorizedTokenRequest(endpoint, Protocol.Default.Version);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			message.SignatureMethod = "PLAINTEXT";
			message.Signature = "cs&ts";
			Assert.IsNotNull(target.ProcessIncomingMessage(message));
		}

		[Test]
		public void HttpsSignatureVerificationNotApplicable() {
			SigningBindingElementBase target = new PlaintextSigningBindingElement();
			target.Channel = new TestChannel();
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("https://localtest", HttpDeliveryMethods.GetRequest);
			ITamperResistantOAuthMessage message = new UnauthorizedTokenRequest(endpoint, Protocol.Default.Version);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			message.SignatureMethod = "ANOTHERALGORITHM";
			message.Signature = "somethingelse";
			Assert.AreEqual(MessageProtections.None, target.ProcessIncomingMessage(message), "PLAINTEXT binding element should opt-out where it doesn't understand.");
		}

		[Test]
		public void HttpSignatureGeneration() {
			SigningBindingElementBase target = new PlaintextSigningBindingElement();
			target.Channel = new TestChannel();
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("http://localtest", HttpDeliveryMethods.GetRequest);
			ITamperResistantOAuthMessage message = new UnauthorizedTokenRequest(endpoint, Protocol.Default.Version);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";

			// Since this is (non-encrypted) HTTP, so the plain text signer should not be used
			Assert.IsNull(target.ProcessOutgoingMessage(message));
			Assert.IsNull(message.SignatureMethod);
			Assert.IsNull(message.Signature);
		}

		[Test]
		public void HttpSignatureVerification() {
			SigningBindingElementBase target = new PlaintextSigningBindingElement();
			target.Channel = new TestChannel();
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("http://localtest", HttpDeliveryMethods.GetRequest);
			ITamperResistantOAuthMessage message = new UnauthorizedTokenRequest(endpoint, Protocol.Default.Version);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			message.SignatureMethod = "PLAINTEXT";
			message.Signature = "cs%26ts";
			Assert.IsNull(target.ProcessIncomingMessage(message), "PLAINTEXT signature binding element should refuse to participate in non-encrypted messages.");
		}
	}
}
