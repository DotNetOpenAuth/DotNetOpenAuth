//-----------------------------------------------------------------------
// <copyright file="StandardExpirationBindingElementTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging.Bindings {
	using System;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class StandardExpirationBindingElementTests : MessagingTestBase {
		[Test]
		public async Task SendSetsTimestamp() {
			TestExpiringMessage message = new TestExpiringMessage(MessageTransport.Indirect);
			message.Recipient = new Uri("http://localtest");
			((IExpiringProtocolMessage)message).UtcCreationDate = DateTime.Parse("1/1/1990");

			Channel channel = CreateChannel(MessageProtections.Expiration);
			await channel.PrepareResponseAsync(message);
			Assert.IsTrue(DateTime.UtcNow - ((IExpiringProtocolMessage)message).UtcCreationDate < TimeSpan.FromSeconds(3), "The timestamp on the message was not set on send.");
		}

		[Test]
		public async Task VerifyGoodTimestampIsAccepted() {
			this.Channel = CreateChannel(MessageProtections.Expiration);
			await this.ParameterizedReceiveProtectedTestAsync(DateTime.UtcNow, false);
		}

		[Test]
		public async Task VerifyFutureTimestampWithinClockSkewIsAccepted() {
			this.Channel = CreateChannel(MessageProtections.Expiration);
			await this.ParameterizedReceiveProtectedTestAsync(DateTime.UtcNow + DotNetOpenAuthSection.Messaging.MaximumClockSkew - TimeSpan.FromSeconds(1), false);
		}

		[Test, ExpectedException(typeof(ExpiredMessageException))]
		public async Task VerifyOldTimestampIsRejected() {
			this.Channel = CreateChannel(MessageProtections.Expiration);
			await this.ParameterizedReceiveProtectedTestAsync(DateTime.UtcNow - StandardExpirationBindingElement.MaximumMessageAge - TimeSpan.FromSeconds(1), false);
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public async Task VerifyFutureTimestampIsRejected() {
			this.Channel = CreateChannel(MessageProtections.Expiration);
			await this.ParameterizedReceiveProtectedTestAsync(DateTime.UtcNow + DotNetOpenAuthSection.Messaging.MaximumClockSkew + TimeSpan.FromSeconds(2), false);
		}
	}
}
