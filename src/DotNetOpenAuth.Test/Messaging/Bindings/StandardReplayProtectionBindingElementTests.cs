//-----------------------------------------------------------------------
// <copyright file="StandardReplayProtectionBindingElementTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging.Bindings {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class StandardReplayProtectionBindingElementTests : MessagingTestBase {
		private Protocol protocol;
		private StandardReplayProtectionBindingElement nonceElement;
		private IReplayProtectedProtocolMessage message;
		private INonceStore nonceStore;

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			this.protocol = Protocol.Default;
			this.nonceStore = new MemoryNonceStore(TimeSpan.FromHours(3));
			this.nonceElement = new StandardReplayProtectionBindingElement(this.nonceStore);
			this.nonceElement.Channel = new Mocks.TestChannel();
			this.message = new TestReplayProtectedMessage();
			this.message.UtcCreationDate = DateTime.UtcNow;
		}

		/// <summary>
		/// Verifies that the generated nonce includes random characters.
		/// </summary>
		[Test]
		public async Task RandomCharactersTest() {
			Assert.IsNotNull(await this.nonceElement.ProcessOutgoingMessageAsync(this.message, CancellationToken.None));
			Assert.IsNotNull(this.message.Nonce, "No nonce was set on the message.");
			Assert.AreNotEqual(0, this.message.Nonce.Length, "The generated nonce was empty.");
			string firstNonce = this.message.Nonce;

			// Apply another nonce and verify that they are different than the first ones.
			Assert.IsNotNull(await this.nonceElement.ProcessOutgoingMessageAsync(this.message, CancellationToken.None));
			Assert.IsNotNull(this.message.Nonce, "No nonce was set on the message.");
			Assert.AreNotEqual(0, this.message.Nonce.Length, "The generated nonce was empty.");
			Assert.AreNotEqual(firstNonce, this.message.Nonce, "The two generated nonces are identical.");
		}

		/// <summary>
		/// Verifies that a message is received correctly.
		/// </summary>
		[Test]
		public async Task ValidMessageReceivedTest() {
			this.message.Nonce = "a";
			Assert.IsNotNull(await this.nonceElement.ProcessIncomingMessageAsync(this.message, CancellationToken.None));
		}

		/// <summary>
		/// Verifies that a message that doesn't have a string of random characters is received correctly.
		/// </summary>
		[Test]
		public async Task ValidMessageNoNonceReceivedTest() {
			this.message.Nonce = string.Empty;
			this.nonceElement.AllowZeroLengthNonce = true;
			Assert.IsNotNull(await this.nonceElement.ProcessIncomingMessageAsync(this.message, CancellationToken.None));
		}

		/// <summary>
		/// Verifies that a message that doesn't have a string of random characters is received correctly.
		/// </summary>
		[Test, ExpectedException(typeof(ProtocolException))]
		public async Task InvalidMessageNoNonceReceivedTest() {
			this.message.Nonce = string.Empty;
			this.nonceElement.AllowZeroLengthNonce = false;
			Assert.IsNotNull(await this.nonceElement.ProcessIncomingMessageAsync(this.message, CancellationToken.None));
		}

		/// <summary>
		/// Verifies that a replayed message is rejected.
		/// </summary>
		[Test, ExpectedException(typeof(ReplayedMessageException))]
		public async Task ReplayDetectionTest() {
			this.message.Nonce = "a";
			Assert.IsNotNull(await this.nonceElement.ProcessIncomingMessageAsync(this.message, CancellationToken.None));

			// Now receive the same message again. This should throw because it's a message replay.
			await this.nonceElement.ProcessIncomingMessageAsync(this.message, CancellationToken.None);
		}
	}
}
