//-----------------------------------------------------------------------
// <copyright file="MemoryCryptoKeyStore.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// A in-memory store of crypto keys.
	/// </summary>
	internal class MemoryCryptoKeyStore : ICryptoKeyStore {
		/// <summary>
		/// How frequently to check for and remove expired secrets.
		/// </summary>
		private static readonly TimeSpan cleaningInterval = TimeSpan.FromMinutes(30);

		/// <summary>
		/// An in-memory cache of decrypted symmetric keys.
		/// </summary>
		/// <remarks>
		/// The key is the bucket name.  The value is a dictionary whose key is the handle and whose value is the cached key.
		/// </remarks>
		private readonly Dictionary<string, Dictionary<string, CryptoKey>> store = new Dictionary<string, Dictionary<string, CryptoKey>>(StringComparer.Ordinal);

		/// <summary>
		/// The last time the cache had expired keys removed from it.
		/// </summary>
		private DateTime lastCleaning = DateTime.UtcNow;

		/// <summary>
		/// Gets the key in a given bucket and handle.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <param name="handle">The key handle.  Case sensitive.</param>
		/// <returns>
		/// The cryptographic key, or <c>null</c> if no matching key was found.
		/// </returns>
		public CryptoKey GetKey(string bucket, string handle) {
			lock (this.store) {
				Dictionary<string, CryptoKey> cacheBucket;
				if (this.store.TryGetValue(bucket, out cacheBucket)) {
					CryptoKey key;
					if (cacheBucket.TryGetValue(handle, out key)) {
						return key;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Gets a sequence of existing keys within a given bucket.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <returns>
		/// A sequence of handles and keys, ordered by descending <see cref="CryptoKey.ExpiresUtc"/>.
		/// </returns>
		public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket) {
			lock (this.store) {
				Dictionary<string, CryptoKey> cacheBucket;
				if (this.store.TryGetValue(bucket, out cacheBucket)) {
					return cacheBucket.ToList();
				} else {
					return Enumerable.Empty<KeyValuePair<string, CryptoKey>>();
				}
			}
		}

		/// <summary>
		/// Stores a cryptographic key.
		/// </summary>
		/// <param name="bucket">The name of the bucket to store the key in.  Case sensitive.</param>
		/// <param name="handle">The handle to the key, unique within the bucket.  Case sensitive.</param>
		/// <param name="key">The key to store.</param>
		/// <exception cref="CryptoKeyCollisionException">Thrown in the event of a conflict with an existing key in the same bucket and with the same handle.</exception>
		public void StoreKey(string bucket, string handle, CryptoKey key) {
			lock (this.store) {
				Dictionary<string, CryptoKey> cacheBucket;
				if (!this.store.TryGetValue(bucket, out cacheBucket)) {
					this.store[bucket] = cacheBucket = new Dictionary<string, CryptoKey>(StringComparer.Ordinal);
				}

				if (cacheBucket.ContainsKey(handle)) {
					throw new CryptoKeyCollisionException();
				}

				cacheBucket[handle] = key;

				this.CleanExpiredKeysFromMemoryCacheIfAppropriate();
			}
		}

		/// <summary>
		/// Removes the key.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <param name="handle">The key handle.  Case sensitive.</param>
		public void RemoveKey(string bucket, string handle) {
			lock (this.store) {
				Dictionary<string, CryptoKey> cacheBucket;
				if (this.store.TryGetValue(bucket, out cacheBucket)) {
					cacheBucket.Remove(handle);
				}
			}
		}

		/// <summary>
		/// Cleans the expired keys from memory cache if the cleaning interval has passed.
		/// </summary>
		private void CleanExpiredKeysFromMemoryCacheIfAppropriate() {
			if (DateTime.UtcNow > this.lastCleaning + cleaningInterval) {
				lock (this.store) {
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
			lock (this.store) {
				var emptyBuckets = new List<string>();
				foreach (var bucketPair in this.store) {
					var expiredKeys = new List<string>();
					foreach (var handlePair in bucketPair.Value) {
						if (handlePair.Value.ExpiresUtc < DateTime.UtcNow) {
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
					this.store.Remove(emptyBucket);
				}

				this.lastCleaning = DateTime.UtcNow;
			}
		}
	}
}
