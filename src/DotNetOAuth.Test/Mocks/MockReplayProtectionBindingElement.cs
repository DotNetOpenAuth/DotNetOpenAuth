//-----------------------------------------------------------------------
// <copyright file="MockReplayProtectionBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	internal class MockReplayProtectionBindingElement : IChannelBindingElement {
		private bool messageReceived;

		#region IChannelBindingElement Members

		MessageProtection IChannelBindingElement.Protection {
			get { return MessageProtection.ReplayProtection; }
		}

		bool IChannelBindingElement.PrepareMessageForSending(IProtocolMessage message) {
			var replayMessage = message as IReplayProtectedProtocolMessage;
			if (replayMessage != null) {
				replayMessage.Nonce = "someNonce";
				return true;
			}

			return false;
		}

		bool IChannelBindingElement.PrepareMessageForReceiving(IProtocolMessage message) {
			var replayMessage = message as IReplayProtectedProtocolMessage;
			if (replayMessage != null) {
				Assert.AreEqual("someNonce", replayMessage.Nonce, "The nonce didn't serialize correctly, or something");
				// this mock implementation passes the first time and fails subsequent times.
				if (this.messageReceived) {
					throw new ReplayedMessageException(message);
				}
				this.messageReceived = true;
				return true;
			}

			return false;
		}

		#endregion
	}
}
