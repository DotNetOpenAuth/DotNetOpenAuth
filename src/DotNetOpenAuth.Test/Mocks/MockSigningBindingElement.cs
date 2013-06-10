//-----------------------------------------------------------------------
// <copyright file="MockSigningBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	internal class MockSigningBindingElement : IChannelBindingElement {
		internal const string MessageSignature = "mocksignature";

		#region IChannelBindingElement Members

		MessageProtections IChannelBindingElement.Protection {
			get { return MessageProtections.TamperProtection; }
		}

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		public Channel Channel { get; set; }

		Task<MessageProtections?> IChannelBindingElement.ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			var signedMessage = message as ITamperResistantProtocolMessage;
			if (signedMessage != null) {
				signedMessage.Signature = MessageSignature;
				return MessageProtectionTasks.TamperProtection;
			}

			return MessageProtectionTasks.Null;
		}

		Task<MessageProtections?> IChannelBindingElement.ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			var signedMessage = message as ITamperResistantProtocolMessage;
			if (signedMessage != null) {
				if (signedMessage.Signature != MessageSignature) {
					throw new InvalidSignatureException(message);
				}
				return MessageProtectionTasks.TamperProtection;
			}

			return MessageProtectionTasks.Null;
		}

		#endregion
	}
}
