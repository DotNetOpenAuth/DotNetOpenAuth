//-----------------------------------------------------------------------
// <copyright file="MockReplayProtectionBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	internal class MockReplayProtectionBindingElement : IChannelBindingElement {
		private bool messageReceived;

		#region IChannelBindingElement Members

		ChannelProtection IChannelBindingElement.Protection {
			get { return ChannelProtection.ReplayProtection; }
		}

		void IChannelBindingElement.PrepareMessageForSending(IProtocolMessage message) {
			var replayMessage = message as IReplayProtectedProtocolMessage;
			if (replayMessage != null) {
				replayMessage.Nonce = "someNonce";
			}
		}

		void IChannelBindingElement.PrepareMessageForReceiving(IProtocolMessage message) {
			var replayMessage = message as IReplayProtectedProtocolMessage;
			if (replayMessage != null) {
				Assert.AreEqual("someNonce", replayMessage.Nonce, "The nonce didn't serialize correctly, or something");
				// this mock implementation passes the first time and fails subsequent times.
				if (this.messageReceived) {
					throw new ReplayedMessageException(message);
				}
				this.messageReceived = true;
			}
		}

		#endregion
	}
}
