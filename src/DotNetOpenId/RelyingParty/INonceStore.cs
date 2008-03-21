using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	public interface INonceStore {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		byte[] SecretSigningKey { get; }

		void StoreNonce(Nonce nonce);
		bool ContainsNonce(Nonce nonce);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nonces")]
		void ClearExpiredNonces();
	}
}
