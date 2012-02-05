//-----------------------------------------------------------------------
// <copyright file="MockReplayProtectionBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using NUnit.Framework;

	internal class MockReplayProtectionBindingElement : IChannelBindingElement {
		private bool messageReceived;

		#region IChannelBindingElement Members

		MessageProtections IChannelBindingElement.Protection {
			get { return MessageProtections.ReplayProtection; }
		}

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		public Channel Channel { get; set; }

		MessageProtections? IChannelBindingElement.ProcessOutgoingMessage(IProtocolMessage message) {
			var replayMessage = message as IReplayProtectedProtocolMessage;
			if (replayMessage != null) {
				replayMessage.Nonce = "someNonce";
				return MessageProtections.ReplayProtection;
			}

			return null;
		}

		MessageProtections? IChannelBindingElement.ProcessIncomingMessage(IProtocolMessage message) {
			var replayMessage = message as IReplayProtectedProtocolMessage;
			if (replayMessage != null) {
				Assert.AreEqual("someNonce", replayMessage.Nonce, "The nonce didn't serialize correctly, or something");
				// this mock implementation passes the first time and fails subsequent times.
				if (this.messageReceived) {
					throw new ReplayedMessageException(message);
				}
				this.messageReceived = true;
				return MessageProtections.ReplayProtection;
			}

			return null;
		}

		#endregion
	}
}
