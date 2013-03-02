namespace OAuth2ProtectedWebApi.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	using DotNetOpenAuth.Messaging.Bindings;

	internal class MemoryCryptoKeyStore : ICryptoKeyStore {
		private Dictionary<string, Dictionary<string, CryptoKey>> keys = new Dictionary<string, Dictionary<string, CryptoKey>>();

		private MemoryCryptoKeyStore() {
		}

		internal static ICryptoKeyStore Instance = new MemoryCryptoKeyStore();

		public CryptoKey GetKey(string bucket, string handle) {
			Dictionary<string, CryptoKey> keyBucket;
			if (this.keys.TryGetValue(bucket, out keyBucket)) {
				CryptoKey key;
				if (keyBucket.TryGetValue(handle, out key)) {
					return key;
				}
			}

			return null;
		}

		public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket) {
			Dictionary<string, CryptoKey> keyBucket;
			if (this.keys.TryGetValue(bucket, out keyBucket)) {
				foreach (var cryptoKey in keyBucket) {
					yield return cryptoKey;
				}
			}
		}

		public void StoreKey(string bucket, string handle, CryptoKey key) {
			Dictionary<string, CryptoKey> keyBucket;
			if (!this.keys.TryGetValue(bucket, out keyBucket)) {
				keyBucket = this.keys[bucket] = new Dictionary<string, CryptoKey>();
			}

			keyBucket[handle] = key;
		}

		public void RemoveKey(string bucket, string handle) {
			Dictionary<string, CryptoKey> keyBucket;
			if (this.keys.TryGetValue(bucket, out keyBucket)) {
				keyBucket.Remove(handle);
			}
		}
	}
}