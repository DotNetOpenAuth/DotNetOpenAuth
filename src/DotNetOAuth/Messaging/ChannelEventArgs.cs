using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetOAuth.Messaging {
	public class ChannelEventArgs : EventArgs {
		internal ChannelEventArgs(IProtocolMessage message) {
			if (message == null) throw new ArgumentNullException("message");

			this.Message = message;
		}

		public IProtocolMessage Message { get; private set; }
	}
}
