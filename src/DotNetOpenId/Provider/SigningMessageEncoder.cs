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
			OnSigning(encodable);
			var response = encodable as EncodableResponse;
			if (response != null) {
				if (response.NeedsSigning) {
					signatory.Sign(response);
				}
			}
			return base.Encode(encodable);
		}

		/// <summary>
		/// Used for testing.  Allows interception and modification of messages 
		/// that are about to be returned to the RP.
		/// </summary>
		public static event EventHandler<EncodeEventArgs> Signing;
		protected virtual void OnSigning(IEncodable encodable) {
			if (Signing != null)
				Signing(this, new EncodeEventArgs(encodable));
		}
	}

}
