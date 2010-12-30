//-----------------------------------------------------------------------
// <copyright file="StandardExpirationBindingElementTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
		[TestCase]
		public void SendSetsTimestamp() {
			TestExpiringMessage message = new TestExpiringMessage(MessageTransport.Indirect);
			message.Recipient = new Uri("http://localtest");
			((IExpiringProtocolMessage)message).UtcCreationDate = DateTime.Parse("1/1/1990");

			Channel channel = CreateChannel(MessageProtections.Expiration);
			channel.PrepareResponse(message);
			Assert.IsTrue(DateTime.UtcNow - ((IExpiringProtocolMessage)message).UtcCreationDate < TimeSpan.FromSeconds(3), "The timestamp on the message was not set on send.");
		}

		[TestCase]
		public void VerifyGoodTimestampIsAccepted() {
			this.Channel = CreateChannel(MessageProtections.Expiration);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow, false);
		}

		[TestCase]
		public void VerifyFutureTimestampWithinClockSkewIsAccepted() {
			this.Channel = CreateChannel(MessageProtections.Expiration);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow + DotNetOpenAuthSection.Configuration.Messaging.MaximumClockSkew - TimeSpan.FromSeconds(1), false);
		}

		[TestCase, ExpectedException(typeof(ExpiredMessageException))]
		public void VerifyOldTimestampIsRejected() {
			this.Channel = CreateChannel(MessageProtections.Expiration);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow - StandardExpirationBindingElement.MaximumMessageAge - TimeSpan.FromSeconds(1), false);
		}

		[TestCase, ExpectedException(typeof(ProtocolException))]
		public void VerifyFutureTimestampIsRejected() {
			this.Channel = CreateChannel(MessageProtections.Expiration);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow + DotNetOpenAuthSection.Configuration.Messaging.MaximumClockSkew + TimeSpan.FromSeconds(2), false);
		}
	}
}
