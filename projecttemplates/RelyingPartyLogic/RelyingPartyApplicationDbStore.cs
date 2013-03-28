//-----------------------------------------------------------------------
// <copyright file="RelyingPartyApplicationDbStore.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using DotNetOpenAuth;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;

	/// <summary>
	/// A database-backed state store for OpenID relying parties.
	/// </summary>
	public class RelyingPartyApplicationDbStore : NonceDbStore, ICryptoKeyAndNonceStore {
		/// <summary>
		/// Initializes a new instance of the <see cref="RelyingPartyApplicationDbStore"/> class.
		/// </summary>
		public RelyingPartyApplicationDbStore() {
		}

		#region ICryptoStore Members

		public CryptoKey GetKey(string bucket, string handle) {
			using (var dataContext = new TransactedDatabaseEntities(System.Data.IsolationLevel.ReadCommitted)) {
				var associations = from assoc in dataContext.SymmetricCryptoKeys
								   where assoc.Bucket == bucket
								   where assoc.Handle == handle
								   where assoc.ExpirationUtc > DateTime.UtcNow
								   select assoc;
				return associations.AsEnumerable()
					.Select(assoc => new CryptoKey(assoc.Secret, assoc.ExpirationUtc.AsUtc()))
					.FirstOrDefault();
			}
		}

		public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket) {
			using (var dataContext = new TransactedDatabaseEntities(System.Data.IsolationLevel.ReadCommitted)) {
				var relevantAssociations = from assoc in dataContext.SymmetricCryptoKeys
										   where assoc.Bucket == bucket
										   where assoc.ExpirationUtc > DateTime.UtcNow
										   orderby assoc.ExpirationUtc descending
										   select assoc;
				var qualifyingAssociations = relevantAssociations.AsEnumerable()
					.Select(assoc => new KeyValuePair<string, CryptoKey>(assoc.Handle, new CryptoKey(assoc.Secret, assoc.ExpirationUtc.AsUtc())));
				return qualifyingAssociations.ToList(); // the data context is closing, so we must cache the result.
			}
		}

		public void StoreKey(string bucket, string handle, CryptoKey key) {
			using (var dataContext = new TransactedDatabaseEntities(System.Data.IsolationLevel.ReadCommitted)) {
				var sharedAssociation = new SymmetricCryptoKey {
					Bucket = bucket,
					Handle = handle,
					ExpirationUtc = key.ExpiresUtc,
					Secret = key.Key,
				};

				dataContext.AddToSymmetricCryptoKeys(sharedAssociation);
			}
		}

		public void RemoveKey(string bucket, string handle) {
			using (var dataContext = new TransactedDatabaseEntities(System.Data.IsolationLevel.ReadCommitted)) {
				var association = dataContext.SymmetricCryptoKeys.FirstOrDefault(a => a.Bucket == bucket && a.Handle == handle);
				if (association != null) {
					dataContext.DeleteObject(association);
				} else {
				}
			}
		}

		#endregion

		/// <summary>
		/// Clears all expired associations from the store.
		/// </summary>
		/// <remarks>
		/// If another algorithm is in place to periodically clear out expired associations,
		/// this method call may be ignored.
		/// This should be done frequently enough to avoid a memory leak, but sparingly enough
		/// to not be a performance drain.
		/// </remarks>
		internal void ClearExpiredCryptoKeys() {
			using (var dataContext = new TransactedDatabaseEntities(IsolationLevel.ReadCommitted)) {
				dataContext.ClearExpiredCryptoKeys(dataContext.Transaction);
			}
		}
	}
}
