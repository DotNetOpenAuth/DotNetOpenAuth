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

	internal class MockSigningBindingElement : IChannelBindingElement {
		internal const string MessageSignature = "mocksignature";

		#region IChannelBindingElement Members

		ChannelProtection IChannelBindingElement.Protection {
			get { return ChannelProtection.TamperProtection; }
		}

		void IChannelBindingElement.PrepareMessageForSending(IProtocolMessage message) {
			ISignedProtocolMessage signedMessage = message as ISignedProtocolMessage;
			if (signedMessage != null) {
				signedMessage.Signature = MessageSignature;
			}
		}

		void IChannelBindingElement.PrepareMessageForReceiving(IProtocolMessage message) {
			ISignedProtocolMessage signedMessage = message as ISignedProtocolMessage;
			if (signedMessage != null) {
				if (signedMessage.Signature != MessageSignature) {
					throw new InvalidSignatureException(message);
				}
			}
		}

		#endregion
	}
}
