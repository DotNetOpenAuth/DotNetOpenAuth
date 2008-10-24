//-----------------------------------------------------------------------
// <copyright file="MockSigningBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;

	internal class MockSigningBindingElement : IChannelBindingElement {
		internal const string MessageSignature = "mocksignature";

		#region IChannelBindingElement Members

		MessageProtections IChannelBindingElement.Protection {
			get { return MessageProtections.TamperProtection; }
		}

		bool IChannelBindingElement.PrepareMessageForSending(IProtocolMessage message) {
			ITamperResistantProtocolMessage signedMessage = message as ITamperResistantProtocolMessage;
			if (signedMessage != null) {
				signedMessage.Signature = MessageSignature;
				return true;
			}

			return false;
		}

		bool IChannelBindingElement.PrepareMessageForReceiving(IProtocolMessage message) {
			ITamperResistantProtocolMessage signedMessage = message as ITamperResistantProtocolMessage;
			if (signedMessage != null) {
				if (signedMessage.Signature != MessageSignature) {
					throw new InvalidSignatureException(message);
				}
				return true;
			}

			return false;
		}

		#endregion
	}
}
