using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId {
	internal enum EncodingType {
		None,
		/// <summary>
		/// Data to be sent to the OP or RP site by telling the user agent to
		/// redirect GET or form POST to a special URL with a payload of arguments.
		/// </summary>
		IndirectMessage,
		/// <summary>
		/// Provider response data to be sent directly to the Relying Party site, 
		/// in response to a direct request initiated by the RP
		/// (not indirect via the user agent).
		/// Key-Value Form encoding will be used.
		/// </summary>
		DirectResponse
	}

	/// <remarks>
	/// Classes that implement IEncodable should be either [Serializable] or
	/// derive from <see cref="MarshalByRefObject"/> so that testing can
	/// remote across app-domain boundaries to sniff/tamper with messages.
	/// </remarks>
	internal interface IEncodable {
		EncodingType EncodingType { get; }
		IDictionary<string, string> EncodedFields { get; }
		Uri RedirectUrl { get; }
		Protocol Protocol { get; }
	}
}
