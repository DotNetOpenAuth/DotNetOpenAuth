using System;
using System.Collections.Generic;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Test.Mocks {
	class DirectMessageSniffWrapper : IDirectMessageChannel {
		IDirectMessageChannel channel;

		internal DirectMessageSniffWrapper(IDirectMessageChannel channel) {
			this.channel = channel;
		}

		internal event Action<ServiceEndpoint, IDictionary<string, string>> Sending;
		internal event Action<ServiceEndpoint, IDictionary<string, string>> Receiving;

		protected virtual void OnSending(ServiceEndpoint provider, IDictionary<string, string> fields) {
			var sending = Sending;
			if (sending != null) {
				sending(provider, fields);
			}
		}

		protected virtual void OnReceiving(ServiceEndpoint provider, IDictionary<string, string> fields) {
			var receiving = Receiving;
			if (receiving != null) {
				receiving(provider, fields);
			}
		}

		#region IDirectMessageChannel Members

		public IDictionary<string, string> SendDirectMessageAndGetResponse(ServiceEndpoint provider, IDictionary<string, string> fields) {
			OnSending(provider, fields);
			var results = channel.SendDirectMessageAndGetResponse(provider, fields);
			OnReceiving(provider, results);
			return results;
		}

		#endregion
	}
}
