using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Consumer {
	/// <summary>
	/// Thrown when immediate mode is used but the provider says it needs setup mode to complete.
	/// </summary>
	/// <remarks>Internal, because never send immediate mode to the provider right now.</remarks>
	internal class SetupNeededException : ProtocolException {
		public Uri ConsumerId { get; private set; }
		public Uri UserSetupUrl { get; private set; }

		public SetupNeededException(Uri consumerId, Uri userSetupUrl)
			: base(string.Empty) {
			ConsumerId = consumerId;
			UserSetupUrl = userSetupUrl;
		}
	}

}
