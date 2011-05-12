//-----------------------------------------------------------------------
// <copyright file="AsymmetricCryptoKeyStoreWrapper.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;

	/// <summary>
	/// Provides RSA encryption of symmetric keys to protect them from a theft of
	/// the persistent store.
	/// </summary>
	public class AsymmetricCryptoKeyStoreWrapper : ICryptoKeyStore {
		/// <summary>
		/// How frequently to check for and remove expired secrets.
		/// </summary>
		private static readonly TimeSpan cleaningInterval = TimeSpan.FromMinutes(30);

		/// <summary>
		/// The persistent store for asymmetrically encrypted symmetric keys.
		/// </summary>
		private readonly ICryptoKeyStore dataStore;

		/// <summary>
		/// The asymmetric algorithm to use encrypting/decrypting the symmetric keys.
		/// </summary>
		private readonly RSACryptoServiceProvider asymmetricCrypto;

		/// <summary>
		/// An in-memory cache of decrypted symmetric keys.
		/// </summary>
		/// <remarks>
		/// The key is the bucket name.  The value is a dictionary whose key is the handle and whose value is the cached key.
		/// </remarks>
		private readonly Dictionary<string, Dictionary<string, CachedCryptoKey>> decryptedKeyCache = new Dictionary<string, Dictionary<string, CachedCryptoKey>>(StringComparer.Ordinal);

		/// <summary>
		/// The last time the cache had expired keys removed from it.
		/// </summary>
		private DateTime lastCleaning = DateTime.UtcNow;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsymmetricCryptoKeyStoreWrapper"/> class.
		/// </summary>
		/// <param name="dataStore">The data store.</param>
		/// <param name="asymmetricCrypto">The asymmetric protection to apply to symmetric keys.  Must include the private key.</param>
		public AsymmetricCryptoKeyStoreWrapper(ICryptoKeyStore dataStore, RSACryptoServiceProvider asymmetricCrypto) {
			Contract.Requires<ArgumentNullException>(dataStore != null, "dataStore");
			Contract.Requires<ArgumentNullException>(asymmetricCrypto != null, "asymmetricCrypto");
			Contract.Requires<ArgumentException>(!asymmetricCrypto.PublicOnly);
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
		public void StoreKey(string bucket, string handle, CryptoKey decryptedCryptoKey) {
			byte[] encryptedKey = this.asymmetricCrypto.Encrypt(decryptedCryptoKey.Key, true);
			var encryptedCryptoKey = new CryptoKey(encryptedKey, decryptedCryptoKey.ExpiresUtc);
			this.dataStore.StoreKey(bucket, handle, encryptedCryptoKey);

			this.CacheKey(bucket, handle, decryptedCryptoKey, encryptedCryptoKey);

			this.CleanExpiredKeysFromMemoryCacheIfAppropriate();
		}

		/// <summary>
		/// Removes the key.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <param name="handle">The key handle.  Case sensitive.</param>
		public void RemoveKey(string bucket, string handle) {
			this.dataStore.RemoveKey(bucket, handle);

			lock (this.decryptedKeyCache) {
				Dictionary<string, CachedCryptoKey> cacheBucket;
				if (this.decryptedKeyCache.TryGetValue(bucket, out cacheBucket)) {
					cacheBucket.Remove(handle);
				}
			}
		}

		/// <summary>
		/// Caches an encrypted/decrypted key pair.
		/// </summary>
		/// <param name="bucket">The bucket.</param>
		/// <param name="handle">The handle.</param>
		/// <param name="decryptedCryptoKey">The decrypted crypto key.</param>
		/// <param name="encryptedCryptoKey">The encrypted crypto key.</param>
		private void CacheKey(string bucket, string handle, CryptoKey decryptedCryptoKey, CryptoKey encryptedCryptoKey) {
			Contract.Requires(!String.IsNullOrEmpty(bucket));
			Contract.Requires(!String.IsNullOrEmpty(handle));
			Contract.Requires(decryptedKeyCache != null);
			Contract.Requires(encryptedCryptoKey != null);

			lock (this.decryptedKeyCache) {
				Dictionary<string, CachedCryptoKey> cacheBucket;
				if (!this.decryptedKeyCache.TryGetValue(bucket, out cacheBucket)) {
					this.decryptedKeyCache[bucket] = cacheBucket = new Dictionary<string, CachedCryptoKey>(StringComparer.Ordinal);
				}

				cacheBucket[handle] = new CachedCryptoKey(encryptedCryptoKey, decryptedCryptoKey);
			}
		}

		/// <summary>
		/// Decrypts the specified key.
		/// </summary>
		/// <param name="encryptedCryptoKey">The encrypted key.</param>
		/// <returns>The decrypted key.</returns>
		private CryptoKey Decrypt(string bucket, string handle, CryptoKey encryptedCryptoKey) {
			if (encryptedCryptoKey == null) {
				return null;
			}

			// Avoid the asymmetric decryption if possible by looking up whether we have that in our cache.
			CachedCryptoKey cached;
			lock (this.decryptedKeyCache) {
				Dictionary<string, CachedCryptoKey> cacheBucket;
				if (this.decryptedKeyCache.TryGetValue(bucket, out cacheBucket)) {
					if (cacheBucket.TryGetValue(handle, out cached) && encryptedCryptoKey.Equals(cached.Encrypted)) {
						return cached.Decrypted;
					}
				}
			}

			byte[] decryptedKey = this.asymmetricCrypto.Decrypt(encryptedCryptoKey.Key, true);
			var decryptedCryptoKey = new CryptoKey(decryptedKey, encryptedCryptoKey.ExpiresUtc);

			// Store the decrypted version in the cache to save time next time.
			this.CacheKey(bucket, handle, decryptedCryptoKey, encryptedCryptoKey);

			return decryptedCryptoKey;
		}

		/// <summary>
		/// Cleans the expired keys from memory cache if the cleaning interval has passed.
		/// </summary>
		private void CleanExpiredKeysFromMemoryCacheIfAppropriate() {
			if (DateTime.UtcNow > this.lastCleaning + cleaningInterval) {
				lock (this.decryptedKeyCache) {
					if (DateTime.UtcNow > this.lastCleaning + cleaningInterval) {
						this.ClearExpiredKeysFromMemoryCache();
					}
				}
			}
		}

		/// <summary>
		/// Weeds out expired keys from the in-memory cache.
		/// </summary>
		private void ClearExpiredKeysFromMemoryCache() {
			lock (this.decryptedKeyCache) {
				var emptyBuckets = new List<string>();
				foreach (var bucketPair in this.decryptedKeyCache) {
					var expiredKeys = new List<string>();
					foreach (var handlePair in bucketPair.Value) {
						if (handlePair.Value.Encrypted.ExpiresUtc < DateTime.UtcNow) {
							expiredKeys.Add(handlePair.Key);
						}
					}

					foreach (var expiredKey in expiredKeys) {
						bucketPair.Value.Remove(expiredKey);
					}

					if (bucketPair.Value.Count == 0) {
						emptyBuckets.Add(bucketPair.Key);
					}
				}

				foreach (string emptyBucket in emptyBuckets) {
					this.decryptedKeyCache.Remove(emptyBucket);
				}

				this.lastCleaning = DateTime.UtcNow;
			}
		}

		/// <summary>
		/// An encrypted key and its decrypted equivalent.
		/// </summary>
		private class CachedCryptoKey {
			/// <summary>
			/// Initializes a new instance of the <see cref="CachedCryptoKey"/> class.
			/// </summary>
			/// <param name="encrypted">The encrypted key.</param>
			/// <param name="decrypted">The decrypted key.</param>
			internal CachedCryptoKey(CryptoKey encrypted, CryptoKey decrypted) {
				Contract.Requires(encrypted != null);
				Contract.Requires(decrypted != null);

				this.Encrypted = encrypted;
				this.Decrypted = decrypted;
			}

			/// <summary>
			/// Gets or sets the encrypted key.
			/// </summary>
			internal CryptoKey Encrypted { get; private set; }

			/// <summary>
			/// Gets or sets the decrypted key.
			/// </summary>
			internal CryptoKey Decrypted { get; private set; }

			/// <summary>
			/// Invariant conditions.
			/// </summary>
			[ContractInvariantMethod]
			[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
			private void ObjectInvariant() {
				Contract.Invariant(this.Encrypted != null);
				Contract.Invariant(this.Decrypted != null);
			}
		}
	}
}
