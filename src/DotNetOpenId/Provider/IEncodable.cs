using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Provider {
	internal enum EncodingType {
		None,
		/// <summary>
		/// Response data to be sent to the consumer web site by telling the 
		/// browser to redirect back to the consumer web site with a querystring
		/// that contains our data.
		/// </summary>
		RedirectBrowserUrl,
		/// <summary>
		/// Response data to be sent directly to the consumer site, 
		/// in response to a direct request initiated by the consumer site
		/// (not the client browser).
		/// </summary>
		ResponseBody
	}

	internal interface IEncodable {
		EncodingType EncodingType { get; }
		IDictionary<string, string> EncodedFields { get; }
		Uri RedirectUrl { get; }
		Protocol Protocol { get; }
	}
}
