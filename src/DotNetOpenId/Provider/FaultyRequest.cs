using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Provider {
	class FaultyRequest : Request {
		public new IEncodable Response { get; private set; }
		internal FaultyRequest(OpenIdProvider provider, IEncodable response)
			: base(provider) {
			Response = response;
		}

		internal override string Mode {
			get { return null; }
		}

		public override bool IsResponseReady {
			get { return true; }
		}

		internal override IEncodable CreateResponse() {
			return Response;
		}
	}
}
