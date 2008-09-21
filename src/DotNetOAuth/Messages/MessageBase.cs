namespace DotNetOAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;

	internal abstract class MessageBase : IProtocolMessage {
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		#region IProtocolMessage Members

		Version IProtocolMessage.ProtocolVersion {
			get { return new Version(1, 0); }
		}

		MessageProtection IProtocolMessage.RequiredProtection {
			get { return this.RequiredProtection; }
		}

		MessageTransport IProtocolMessage.Transport {
			get { return this.Transport; }
		}

		IDictionary<string, string> IProtocolMessage.ExtraData {
			get { return this.extraData; }
		}

		void IProtocolMessage.EnsureValidMessage() {
			this.EnsureValidMessage();
		}

		#endregion

		protected abstract MessageTransport Transport { get; }

		protected abstract MessageProtection RequiredProtection { get; }

		protected virtual void EnsureValidMessage() { }
	}
}
