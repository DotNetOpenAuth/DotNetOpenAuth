//-----------------------------------------------------------------------
// <copyright file="StandardReplayProtectionBindingElementTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging.Bindings {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class StandardReplayProtectionBindingElementTests : MessagingTestBase {
		private Protocol protocol;
		private StandardReplayProtectionBindingElement nonceElement;
		private IReplayProtectedProtocolMessage message;
		private INonceStore nonceStore;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.protocol = Protocol.Default;
			this.nonceStore = new NonceMemoryStore(TimeSpan.FromHours(3));
			this.nonceElement = new StandardReplayProtectionBindingElement(this.nonceStore);
			this.message = new TestReplayProtectedMessage();
			this.message.UtcCreationDate = DateTime.UtcNow;
		}

		/// <summary>
		/// Verifies that the generated nonce includes random characters.
		/// </summary>
		[TestMethod]
		public void RandomCharactersTest() {
			Assert.IsNotNull(this.nonceElement.ProcessOutgoingMessage(this.message));
			Assert.IsNotNull(this.message.Nonce, "No nonce was set on the message.");
			Assert.AreNotEqual(0, this.message.Nonce.Length, "The generated nonce was empty.");
			string firstNonce = this.message.Nonce;

			// Apply another nonce and verify that they are different than the first ones.
			Assert.IsNotNull(this.nonceElement.ProcessOutgoingMessage(this.message));
			Assert.IsNotNull(this.message.Nonce, "No nonce was set on the message.");
			Assert.AreNotEqual(0, this.message.Nonce.Length, "The generated nonce was empty.");
			Assert.AreNotEqual(firstNonce, this.message.Nonce, "The two generated nonces are identical.");
		}

		/// <summary>
		/// Verifies that a message is received correctly.
		/// </summary>
		[TestMethod]
		public void ValidMessageReceivedTest() {
			this.message.Nonce = "a";
			Assert.IsNotNull(this.nonceElement.ProcessIncomingMessage(this.message));
		}

		/// <summary>
		/// Verifies that a message that doesn't have a string of random characters is received correctly.
		/// </summary>
		[TestMethod]
		public void ValidMessageNoNonceReceivedTest() {
			this.message.Nonce = string.Empty;
			this.nonceElement.AllowZeroLengthNonce = true;
			Assert.IsNotNull(this.nonceElement.ProcessIncomingMessage(this.message));
		}

		/// <summary>
		/// Verifies that a message that doesn't have a string of random characters is received correctly.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void InvalidMessageNoNonceReceivedTest() {
			this.message.Nonce = string.Empty;
			this.nonceElement.AllowZeroLengthNonce = false;
			Assert.IsNotNull(this.nonceElement.ProcessIncomingMessage(this.message));
		}

		/// <summary>
		/// Verifies that a replayed message is rejected.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ReplayedMessageException))]
		public void ReplayDetectionTest() {
			this.message.Nonce = "a";
			Assert.IsNotNull(this.nonceElement.ProcessIncomingMessage(this.message));

			// Now receive the same message again. This should throw because it's a message replay.
			this.nonceElement.ProcessIncomingMessage(this.message);
		}
	}
}
