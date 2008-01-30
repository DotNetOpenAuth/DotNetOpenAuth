using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Web;
using System.Web.Caching;
using Janrain.OpenId;


namespace Janrain.OpenId.Store {

	public class MemoryStore : IAssociationStore {

		#region Member Variables

		Dictionary<Uri, ServerAssocs> serverAssocsTable = new Dictionary<Uri, ServerAssocs>();
		Hashtable nonces = new Hashtable();
		byte[] authKey;

		#endregion

		#region Methods

		public ServerAssocs GetServerAssocs(Uri server_url) {
			lock (this) {
				if (!serverAssocsTable.ContainsKey(server_url)) {
					serverAssocsTable.Add(server_url, new ServerAssocs());
				}

				return serverAssocsTable[server_url];
			}
		}

		public void StoreAssociation(Uri server_url, Association assoc) {
			lock (this) {
				if (!serverAssocsTable.ContainsKey(server_url))
					serverAssocsTable.Add(server_url, new ServerAssocs());

				ServerAssocs server_assocs = serverAssocsTable[server_url];

				server_assocs.Set(assoc);
			}
		}

		public Association GetAssociation(Uri serverUri) {
			lock (this) {
				return GetServerAssocs(serverUri).Best();
			}
		}

		public Association GetAssociation(Uri serverUri, string handle) {
			lock (this) {
				return GetServerAssocs(serverUri).Get(handle);
			}
		}

		public bool RemoveAssociation(Uri serverUri, string handle) {
			lock (this) {
				return GetServerAssocs(serverUri).Remove(handle);
			}
		}

		public bool StoreNonce(string nonce) {
			lock (this) {
				bool present = nonces.ContainsKey(nonce);

				nonces.Remove(nonce);

				return present;

			}
		}

		public bool UseNonce(string nonce) {
			lock (this) {
				bool present = nonces.ContainsKey(nonce);

				nonces.Remove(nonce);

				return present;

			}
		}

		#endregion

		#region IAssociationStore Members

		byte[] IAssociationStore.AuthKey {
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

		bool IAssociationStore.IsDumb {
			get { return false; }
		}

		void IAssociationStore.StoreNonce(string nonce) {
			this.StoreNonce(nonce);
		}

		#endregion

	}
}
