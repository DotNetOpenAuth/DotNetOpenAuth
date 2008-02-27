using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Provider {
	class FaultyRequest : Request {
		public OpenIdException Exception { get; private set; }
		internal FaultyRequest(OpenIdProvider server, OpenIdException ex)
			: base(server) {
			Exception = ex;
		}

		internal override string Mode {
			get { return null; }
		}

		public override bool IsResponseReady {
			get { return true; }
		}

		internal override IEncodable CreateResponse() {
			return Exception;
		}
	}
}
