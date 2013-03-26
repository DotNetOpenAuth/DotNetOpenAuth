//-----------------------------------------------------------------------
// <copyright file="MockReplayProtectionBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System.Threading;
	using System.Threading.Tasks;

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

		Task<MessageProtections?> IChannelBindingElement.ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			var replayMessage = message as IReplayProtectedProtocolMessage;
			if (replayMessage != null) {
				replayMessage.Nonce = "someNonce";
				return MessageProtectionTasks.ReplayProtection;
			}

			return MessageProtectionTasks.Null;
		}

		Task<MessageProtections?> IChannelBindingElement.ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			var replayMessage = message as IReplayProtectedProtocolMessage;
			if (replayMessage != null) {
				Assert.AreEqual("someNonce", replayMessage.Nonce, "The nonce didn't serialize correctly, or something");
				// this mock implementation passes the first time and fails subsequent times.
				if (this.messageReceived) {
					throw new ReplayedMessageException(message);
				}
				this.messageReceived = true;
				return MessageProtectionTasks.ReplayProtection;
			}

			return MessageProtectionTasks.Null;
		}

		#endregion
	}
}
