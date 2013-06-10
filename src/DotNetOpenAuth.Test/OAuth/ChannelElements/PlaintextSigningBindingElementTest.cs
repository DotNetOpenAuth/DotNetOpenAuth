//-----------------------------------------------------------------------
// <copyright file="PlaintextSigningBindingElementTest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth.ChannelElements {
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class PlaintextSigningBindingElementTest {
		[Test]
		public async Task HttpsSignatureGeneration() {
			SigningBindingElementBase target = new PlaintextSigningBindingElement();
			target.Channel = new TestChannel();
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("https://localtest", HttpDeliveryMethods.GetRequest);
			ITamperResistantOAuthMessage message = new UnauthorizedTokenRequest(endpoint, Protocol.Default.Version);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			Assert.IsNotNull(await target.ProcessOutgoingMessageAsync(message, CancellationToken.None));
			Assert.AreEqual("PLAINTEXT", message.SignatureMethod);
			Assert.AreEqual("cs&ts", message.Signature);
		}

		[Test]
		public async Task HttpsSignatureVerification() {
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("https://localtest", HttpDeliveryMethods.GetRequest);
			ITamperProtectionChannelBindingElement target = new PlaintextSigningBindingElement();
			target.Channel = new TestChannel();
			ITamperResistantOAuthMessage message = new UnauthorizedTokenRequest(endpoint, Protocol.Default.Version);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			message.SignatureMethod = "PLAINTEXT";
			message.Signature = "cs&ts";
			Assert.IsNotNull(target.ProcessIncomingMessageAsync(message, CancellationToken.None));
		}

		[Test]
		public async Task HttpsSignatureVerificationNotApplicable() {
			SigningBindingElementBase target = new PlaintextSigningBindingElement();
			target.Channel = new TestChannel();
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("https://localtest", HttpDeliveryMethods.GetRequest);
			ITamperResistantOAuthMessage message = new UnauthorizedTokenRequest(endpoint, Protocol.Default.Version);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			message.SignatureMethod = "ANOTHERALGORITHM";
			message.Signature = "somethingelse";
			Assert.AreEqual(MessageProtections.None, await target.ProcessIncomingMessageAsync(message, CancellationToken.None), "PLAINTEXT binding element should opt-out where it doesn't understand.");
		}

		[Test]
		public async Task HttpSignatureGeneration() {
			SigningBindingElementBase target = new PlaintextSigningBindingElement();
			target.Channel = new TestChannel();
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("http://localtest", HttpDeliveryMethods.GetRequest);
			ITamperResistantOAuthMessage message = new UnauthorizedTokenRequest(endpoint, Protocol.Default.Version);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";

			// Since this is (non-encrypted) HTTP, so the plain text signer should not be used
			Assert.IsNull(await target.ProcessOutgoingMessageAsync(message, CancellationToken.None));
			Assert.IsNull(message.SignatureMethod);
			Assert.IsNull(message.Signature);
		}

		[Test]
		public async Task HttpSignatureVerification() {
			SigningBindingElementBase target = new PlaintextSigningBindingElement();
			target.Channel = new TestChannel();
			MessageReceivingEndpoint endpoint = new MessageReceivingEndpoint("http://localtest", HttpDeliveryMethods.GetRequest);
			ITamperResistantOAuthMessage message = new UnauthorizedTokenRequest(endpoint, Protocol.Default.Version);
			message.ConsumerSecret = "cs";
			message.TokenSecret = "ts";
			message.SignatureMethod = "PLAINTEXT";
			message.Signature = "cs%26ts";
			Assert.IsNull(await target.ProcessIncomingMessageAsync(message, CancellationToken.None), "PLAINTEXT signature binding element should refuse to participate in non-encrypted messages.");
		}
	}
}
