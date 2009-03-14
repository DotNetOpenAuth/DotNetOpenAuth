using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.ChannelElements;

/// <summary>
/// Summary description for SignatureTamperingBindingElement
/// </summary>
public class SignatureTamperingBindingElement : IChannelBindingElement {
	public SignatureTamperingBindingElement() {
	}

	public bool InvalidateSignature { get; set; }

	#region IChannelBindingElement Members

	public Channel Channel { get; set; }

	public MessageProtections Protection {
		get { return MessageProtections.None; }
	}

	public MessageProtections? PrepareMessageForSending(IProtocolMessage message) {
		if (this.InvalidateSignature) {
			var signedMessage = message as ITamperResistantOpenIdMessage;
			byte[] signature = Convert.FromBase64String(signedMessage.Signature);
			unchecked { signature[0]++; }
			signedMessage.Signature = Convert.ToBase64String(signature);
			return MessageProtections.None;
		} else {
			return null;
		}
	}

	public MessageProtections? PrepareMessageForReceiving(IProtocolMessage message) {
		throw new NotImplementedException();
	}

	#endregion
}
