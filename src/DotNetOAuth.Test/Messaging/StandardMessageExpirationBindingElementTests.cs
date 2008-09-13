//-----------------------------------------------------------------------
// <copyright file="StandardMessageExpirationBindingElementTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Messaging {
	using System;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class StandardMessageExpirationBindingElementTests : MessagingTestBase {
		[TestMethod]
		public void SendSetsTimestamp() {
			TestExpiringMessage message = new TestExpiringMessage(MessageTransport.Indirect);
			message.Recipient = new Uri("http://localtest");
			((IExpiringProtocolMessage)message).UtcCreationDate = DateTime.Parse("1/1/1990");

			Channel channel = CreateChannel(ChannelProtection.Expiration, ChannelProtection.Expiration);
			channel.Send(message);
			Assert.IsTrue(DateTime.UtcNow - ((IExpiringProtocolMessage)message).UtcCreationDate < TimeSpan.FromSeconds(3), "The timestamp on the message was not set on send.");
		}

		[TestMethod]
		public void VerifyGoodTimestampIsAccepted() {
			this.Channel = CreateChannel(ChannelProtection.Expiration, ChannelProtection.Expiration);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow, false);
		}

		[TestMethod, ExpectedException(typeof(ExpiredMessageException))]
		public void VerifyBadTimestampIsRejected() {
			this.Channel = CreateChannel(ChannelProtection.Expiration, ChannelProtection.Expiration);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow - StandardMessageExpirationBindingElement.DefaultMaximumMessageAge - TimeSpan.FromSeconds(1), false);
		}
	}
}
