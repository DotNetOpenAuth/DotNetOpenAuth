//-----------------------------------------------------------------------
// <copyright file="AsymmetricCryptoKeyStoreWrapper.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Provides RSA encryption of symmetric keys to protect them from a theft of
	/// the persistent store.
	/// </summary>
	public class AsymmetricCryptoKeyStoreWrapper : ICryptoKeyStore {
		/// <summary>
		/// The persistent store for asymmetrically encrypted symmetric keys.
		/// </summary>
		private readonly ICryptoKeyStore dataStore;

		/// <summary>
		/// The memory cache of decrypted keys.
		/// </summary>
		private readonly MemoryCryptoKeyStore cache = new MemoryCryptoKeyStore();

		/// <summary>
		/// The asymmetric algorithm to use encrypting/decrypting the symmetric keys.
		/// </summary>
		private readonly RSACryptoServiceProvider asymmetricCrypto;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsymmetricCryptoKeyStoreWrapper"/> class.
		/// </summary>
		/// <param name="dataStore">The data store.</param>
		/// <param name="asymmetricCrypto">The asymmetric protection to apply to symmetric keys.  Must include the private key.</param>
		public AsymmetricCryptoKeyStoreWrapper(ICryptoKeyStore dataStore, RSACryptoServiceProvider asymmetricCrypto) {
			Requires.NotNull(dataStore, "dataStore");
			Requires.NotNull(asymmetricCrypto, "asymmetricCrypto");
			Requires.That(!asymmetricCrypto.PublicOnly, "asymmetricCrypto", "Private key required.");
			this.dataStore = dataStore;
			this.asymmetricCrypto = asymmetricCrypto;
		}

		/// <summary>
		/// Gets the key in a given bucket and handle.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <param name="handle">The key handle.  Case sensitive.</param>
		/// <returns>
		/// The cryptographic key, or <c>null</c> if no matching key was found.
		/// </returns>
		public CryptoKey GetKey(string bucket, string handle) {
			var key = this.dataStore.GetKey(bucket, handle);
			return this.Decrypt(bucket, handle, key);
		}

		/// <summary>
		/// Gets a sequence of existing keys within a given bucket.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <returns>
		/// A sequence of handles and keys, ordered by descending <see cref="CryptoKey.ExpiresUtc"/>.
		/// </returns>
		public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket) {
			return this.dataStore.GetKeys(bucket)
				.Select(pair => new KeyValuePair<string, CryptoKey>(pair.Key, this.Decrypt(bucket, pair.Key, pair.Value)));
		}

		/// <summary>
		/// Stores a cryptographic key.
		/// </summary>
		/// <param name="bucket">The name of the bucket to store the key in.  Case sensitive.</param>
		/// <param name="handle">The handle to the key, unique within the bucket.  Case sensitive.</param>
		/// <param name="decryptedCryptoKey">The key to store.</param>
		[SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "2#", Justification = "Helps readability because multiple keys are involved.")]
		public void StoreKey(string bucket, string handle, CryptoKey decryptedCryptoKey) {
			byte[] encryptedKey = this.asymmetricCrypto.Encrypt(decryptedCryptoKey.Key, true);
			var encryptedCryptoKey = new CryptoKey(encryptedKey, decryptedCryptoKey.ExpiresUtc);
			this.dataStore.StoreKey(bucket, handle, encryptedCryptoKey);

			this.cache.StoreKey(bucket, handle, new CachedCryptoKey(encryptedCryptoKey, decryptedCryptoKey));
		}

		/// <summary>
		/// Removes the key.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <param name="handle">The key handle.  Case sensitive.</param>
		public void RemoveKey(string bucket, string handle) {
			this.dataStore.RemoveKey(bucket, handle);
			this.cache.RemoveKey(bucket, handle);
		}

		/// <summary>
		/// Decrypts the specified key.
		/// </summary>
		/// <param name="bucket">The bucket.</param>
		/// <param name="handle">The handle.</param>
		/// <param name="encryptedCryptoKey">The encrypted key.</param>
		/// <returns>
		/// The decrypted key.
		/// </returns>
		[SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "2#", Justification = "Helps readability because multiple keys are involved.")]
		private CryptoKey Decrypt(string bucket, string handle, CryptoKey encryptedCryptoKey) {
			if (encryptedCryptoKey == null) {
				return null;
			}

			// Avoid the asymmetric decryption if possible by looking up whether we have that in our cache.
			CachedCryptoKey cached = (CachedCryptoKey)this.cache.GetKey(bucket, handle);
			if (cached != null && MessagingUtilities.AreEquivalent(cached.EncryptedKey, encryptedCryptoKey.Key)) {
				return cached;
			}

			byte[] decryptedKey = this.asymmetricCrypto.Decrypt(encryptedCryptoKey.Key, true);
			var decryptedCryptoKey = new CryptoKey(decryptedKey, encryptedCryptoKey.ExpiresUtc);

			// Store the decrypted version in the cache to save time next time.
			this.cache.StoreKey(bucket, handle, new CachedCryptoKey(encryptedCryptoKey, decryptedCryptoKey));

			return decryptedCryptoKey;
		}

		/// <summary>
		/// An encrypted key and its decrypted equivalent.
		/// </summary>
		private class CachedCryptoKey : CryptoKey {
			/// <summary>
			/// Initializes a new instance of the <see cref="CachedCryptoKey"/> class.
			/// </summary>
			/// <param name="encrypted">The encrypted key.</param>
			/// <param name="decrypted">The decrypted key.</param>
			internal CachedCryptoKey(CryptoKey encrypted, CryptoKey decrypted)
				: base(decrypted.Key, decrypted.ExpiresUtc) {
				Requires.NotNull(encrypted, "encrypted");
				Requires.NotNull(decrypted, "decrypted");
				Requires.That(encrypted.ExpiresUtc == decrypted.ExpiresUtc, "encrypted", "encrypted and decrypted expirations must equal.");

				this.EncryptedKey = encrypted.Key;
			}

			/// <summary>
			/// Gets the encrypted key.
			/// </summary>
			internal byte[] EncryptedKey { get; private set; }
		}
	}
}
