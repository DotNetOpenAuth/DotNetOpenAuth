using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace DotNetOpenId.Consumer {
	internal class ConsumerApplicationMemoryStore : AssociationMemoryStore<Uri>, IConsumerApplicationStore {
		#region IConsumerApplicationStore Members

		byte[] secretSigningKey;
		public byte[] SecretSigningKey {
			get {
				if (secretSigningKey == null) {
					lock (this) {
						if (secretSigningKey == null) {
							// initialize in a local variable before setting in field for thread safety.
							byte[] auth_key = new byte[20];
							new RNGCryptoServiceProvider().GetBytes(auth_key);
							this.secretSigningKey = auth_key;
						}
					}
				}
				return secretSigningKey;
			}
		}

		List<Nonce> nonces = new List<Nonce>();

		public void StoreNonce(Nonce nonce) {
			lock (this) {
				nonces.Add(nonce);
			}
		}

		public bool ContainsNonce(Nonce nonce) {
			return nonces.Contains(nonce);
		}

		public void ClearExpiredNonces() {
			lock (this) {
				List<Nonce> expireds = new List<Nonce>(nonces.Count);
				foreach (Nonce nonce in nonces)
					if (nonce.IsExpired)
						expireds.Add(nonce);
				foreach (Nonce nonce in expireds)
					nonces.Remove(nonce);
			}
		}

		#endregion
	}
}
