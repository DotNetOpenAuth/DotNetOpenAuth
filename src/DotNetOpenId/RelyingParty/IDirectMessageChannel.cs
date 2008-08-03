using System.Collections.Generic;

namespace DotNetOpenId.RelyingParty {
	internal interface IDirectMessageChannel {
		IDictionary<string, string> SendDirectMessageAndGetResponse(ServiceEndpoint provider, IDictionary<string, string> fields);
	}
}
