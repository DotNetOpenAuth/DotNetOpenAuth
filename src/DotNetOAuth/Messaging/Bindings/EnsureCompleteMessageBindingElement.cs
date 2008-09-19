using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOAuth.Messaging.Reflection;

namespace DotNetOAuth.Messaging.Bindings {
	internal class EnsureCompleteMessageBindingElement : IChannelBindingElement {
		#region IChannelBindingElement Members

		public MessageProtection Protection {
			get { return MessageProtection.None; }
		}

		public bool PrepareMessageForSending(IProtocolMessage message) {
			// Before we're sending the message, make sure all the required parts are present.
			MessageDictionary dictionary = new MessageDictionary(message);
			MessageDescription.Get(message.GetType()).EnsureRequiredMessagePartsArePresent(dictionary.Keys);
			return true;
		}

		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			// Once the message is deserialized, it is too late to use the MessageDictionary
			// to see if all the message parts were included in the original serialized message
			// because non-nullable value types will already have default values by now.
			// The code for verifying complete incoming messages is included in 
			// MessageSerializer.Deserialize.
			return false;
		}

		#endregion
	}
}
