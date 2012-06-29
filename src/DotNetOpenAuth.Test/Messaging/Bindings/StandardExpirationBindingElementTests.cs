//-----------------------------------------------------------------------
// <copyright file="StandardExpirationBindingElementTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging.Bindings {
	using System;

	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class StandardExpirationBindingElementTests : MessagingTestBase {
		[Test]
		public void SendSetsTimestamp() {
			TestExpiringMessage message = new TestExpiringMessage(MessageTransport.Indirect);
			message.Recipient = new Uri("http://localtest");
			((IExpiringProtocolMessage)message).UtcCreationDate = DateTime.Parse("1/1/1990");

			Channel channel = CreateChannel(MessageProtections.Expiration);
			channel.PrepareResponse(message);
			Assert.IsTrue(DateTime.UtcNow - ((IExpiringProtocolMessage)message).UtcCreationDate < TimeSpan.FromSeconds(3), "The timestamp on the message was not set on send.");
		}

		[Test]
		public void VerifyGoodTimestampIsAccepted() {
			this.Channel = CreateChannel(MessageProtections.Expiration);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow, false);
		}

		[Test]
		public void VerifyFutureTimestampWithinClockSkewIsAccepted() {
			this.Channel = CreateChannel(MessageProtections.Expiration);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow + DotNetOpenAuthSection.Messaging.MaximumClockSkew - TimeSpan.FromSeconds(1), false);
		}

		[Test, ExpectedException(typeof(ExpiredMessageException))]
		public void VerifyOldTimestampIsRejected() {
			this.Channel = CreateChannel(MessageProtections.Expiration);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow - StandardExpirationBindingElement.MaximumMessageAge - TimeSpan.FromSeconds(1), false);
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public void VerifyFutureTimestampIsRejected() {
			this.Channel = CreateChannel(MessageProtections.Expiration);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow + DotNetOpenAuthSection.Messaging.MaximumClockSkew + TimeSpan.FromSeconds(2), false);
		}
	}
}
