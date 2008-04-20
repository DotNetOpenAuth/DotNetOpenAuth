using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// The contract for recalling nonces during their useful lifetime.
	/// </summary>
	public interface INonceStore {
		/// <summary>
		/// Gets some key that can be used for signing.  Any positive length can be used, but a
		/// length of 64 bytes is recommended.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		byte[] SecretSigningKey { get; }

		/// <summary>
		/// Stores a nonce at least until it expires.
		/// </summary>
		/// <returns>
		/// True if the nonce was stored and did not exist previous to this call.
		/// False if the nonce already exists in the store.
		/// </returns>
		/// <remarks>
		/// <para>When persisting nonce instances, only the <see cref="Nonce.Code"/> and <see cref="Nonce.ExpirationDate"/>
		/// properties are significant.  The Code property is used for checking prior nonce use,
		/// and the ExpirationDate for rapid deletion of expired nonces.</para>
		/// <para>Nonces never need to be deserialized.</para>
		/// <para>When checking if a nonce already exists, only the Nonce.Code field should be compared.</para>
		/// <para>Checking for the prior existence of the given nonce and adding the nonce if it 
		/// did not previously exist must be an atomic operation to prevent replay attacks
		/// in the race condition of two threads trying to store the same nonce at the same time.
		/// This should be done by using a UNIQUE constraint on the Nonce.Code column, and perhaps
		/// a transaction that guarantees repeatable READ operations to ensure that no other process
		/// can add a given nonce once you've verified that it's not there.</para>
		/// </remarks>
		bool TryStoreNonce(Nonce nonce);
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
