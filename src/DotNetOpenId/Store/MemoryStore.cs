using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Web;
using System.Web.Caching;
using DotNetOpenId;


namespace DotNetOpenId.Store {

	internal class AssociationMemoryStore<TKey> : IAssociationStore<TKey> {
		Dictionary<TKey, Associations> serverAssocsTable = new Dictionary<TKey, Associations>();
		byte[] authKey;

		internal Associations GetServerAssocs(TKey distinguishingFactor) {
			lock (this) {

				if (!serverAssocsTable.ContainsKey(distinguishingFactor)) {
					serverAssocsTable.Add(distinguishingFactor, new Associations());
				}

				return serverAssocsTable[distinguishingFactor];
			}
		}

		public void StoreAssociation(TKey distinguishingFactor, Association assoc) {
			lock (this) {
				if (!serverAssocsTable.ContainsKey(distinguishingFactor))
					serverAssocsTable.Add(distinguishingFactor, new Associations());

				Associations server_assocs = serverAssocsTable[distinguishingFactor];

				server_assocs.Set(assoc);
			}
		}

		public Association GetAssociation(TKey distinguishingFactor) {
			lock (this) {
				return GetServerAssocs(distinguishingFactor).Best;
			}
		}

		public Association GetAssociation(TKey distinguishingFactor, string handle) {
			lock (this) {
				return GetServerAssocs(distinguishingFactor).Get(handle);
			}
		}

		public bool RemoveAssociation(TKey distinguishingFactor, string handle) {
			lock (this) {
				return GetServerAssocs(distinguishingFactor).Remove(handle);
			}
		}

		#region IAssociationStore Members

		byte[] IAssociationStore<TKey>.AuthKey {
			get {
				if (authKey == null) {
					lock (this) {
						if (authKey == null) {
							// initialize in a local variable before setting in field for thread safety.
							byte[] auth_key = new byte[20];
							new RNGCryptoServiceProvider().GetBytes(auth_key);
							this.authKey = auth_key;
						}
					}
				}
				return authKey;
			}
		}

		#endregion
	}
}
