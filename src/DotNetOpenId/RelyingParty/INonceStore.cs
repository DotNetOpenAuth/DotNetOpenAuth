using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	public interface INonceStore {
		/// <summary>
		/// Gets some key that can be used for signing.  Any positive length can be used, but a
		/// length of 64 bytes is recommended.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		byte[] SecretSigningKey { get; }

		/// <summary>
		/// Stores a nonce.  Checking for an existing nonce with the same <see cref="Nonce.Code"/> value
		/// is not necessary as <see cref="ContainsNonce"/> is atomically checked first.
		/// </summary>
		/// <remarks>
		/// When persisting nonce instances, only the <see cref="Nonce.Code"/> and <see cref="Nonce.ExpirationDate"/>
		/// properties are significant.  Nonces never need to be deserialized.
		/// </remarks>
		void StoreNonce(Nonce nonce);
		/// <summary>
		/// Gets whether a given nonce already exists in the store.
		/// </summary>
		/// <remarks>
		/// When checking a persistent store for an existing nonce, only compare the
		/// <see cref="Nonce.Code"/> fields.
		/// </remarks>
		bool ContainsNonce(Nonce nonce);
		/// <summary>
		/// Hints to the store to clear expired nonces.
		/// </summary>
		/// <remarks>
		/// If another algorithm is in place to periodically clear out expired nonces,
		/// this method call may be ignored.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nonces")]
		void ClearExpiredNonces();
	}
}
