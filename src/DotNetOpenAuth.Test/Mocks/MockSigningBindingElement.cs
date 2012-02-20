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

		MessageProtections? IChannelBindingElement.ProcessOutgoingMessage(IProtocolMessage message) {
			ITamperResistantProtocolMessage signedMessage = message as ITamperResistantProtocolMessage;
			if (signedMessage != null) {
				signedMessage.Signature = MessageSignature;
				return MessageProtections.TamperProtection;
			}

			return null;
		}

		MessageProtections? IChannelBindingElement.ProcessIncomingMessage(IProtocolMessage message) {
			ITamperResistantProtocolMessage signedMessage = message as ITamperResistantProtocolMessage;
			if (signedMessage != null) {
				if (signedMessage.Signature != MessageSignature) {
					throw new InvalidSignatureException(message);
				}
				return MessageProtections.TamperProtection;
			}

			return null;
		}

		#endregion
	}
}
