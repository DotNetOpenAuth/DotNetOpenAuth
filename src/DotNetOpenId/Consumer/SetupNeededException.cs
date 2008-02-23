using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Consumer {
	public class SetupNeededException : ProtocolException {
		public Uri ConsumerId { get; private set; }
		public Uri UserSetupUrl { get; private set; }

		public SetupNeededException(Uri consumerId, Uri userSetupUrl)
			: base(string.Empty) {
			ConsumerId = consumerId;
			UserSetupUrl = userSetupUrl;
		}
	}

}
