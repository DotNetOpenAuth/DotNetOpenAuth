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

		MessageProtection IChannelBindingElement.Protection {
			get { return MessageProtection.TamperProtection; }
		}

		void IChannelBindingElement.PrepareMessageForSending(IProtocolMessage message) {
			ISignedOAuthMessage signedMessage = message as ISignedOAuthMessage;
			if (signedMessage != null) {
				signedMessage.Signature = MessageSignature;
			}
		}

		void IChannelBindingElement.PrepareMessageForReceiving(IProtocolMessage message) {
			ISignedOAuthMessage signedMessage = message as ISignedOAuthMessage;
			if (signedMessage != null) {
				if (signedMessage.Signature != MessageSignature) {
					throw new InvalidSignatureException(message);
				}
			}
		}

		#endregion
	}
}
