using System;
using System.Diagnostics;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// Encodes responses in to <see cref="Response"/>, signing them when required.
	/// </summary>
	internal class SigningMessageEncoder : MessageEncoder {
		Signatory signatory;

		public SigningMessageEncoder(Signatory signatory) {
			if (signatory == null)
				throw new ArgumentNullException("signatory", "Must have a store to sign this request");

			this.signatory = signatory;
		}

		public override Response Encode(IEncodable encodable) {
			var response = encodable as EncodableResponse;
			if (response != null) {
				if (response.NeedsSigning) {
					signatory.Sign(response);
				}
			}
			return base.Encode(encodable);
		}
	}

}
