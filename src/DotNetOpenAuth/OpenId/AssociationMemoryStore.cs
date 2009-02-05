//-----------------------------------------------------------------------
// <copyright file="AssociationMemoryStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Manages a set of associations in memory only (no database).
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <remarks>
	/// This class should be used for low-to-medium traffic relying party sites that can afford to lose associations
	/// if the app pool was ever restarted.  High traffic relying parties and providers should write their own
	/// implementation of <see cref="IAssociationStore&lt;TKey&gt;"/> that works against their own database schema
	/// to allow for persistance and recall of associations across servers in a web farm and server restarts.
	/// </remarks>
	internal class AssociationMemoryStore<TKey> : IAssociationStore<TKey> {
		/// <summary>
		/// For Relying Parties, this maps OP Endpoints to a set of associations with that endpoint.
		/// For Providers, this keeps smart and dumb associations in two distinct pools.
		/// </summary>
		private Dictionary<TKey, Associations> serverAssocsTable = new Dictionary<TKey, Associations>();

		/// <summary>
		/// Stores a given association for later recall.
		/// </summary>
		/// <param name="distinguishingFactor">The distinguishing factor, either an OP Endpoint or smart/dumb mode.</param>
		/// <param name="association">The association to store.</param>
		public void StoreAssociation(TKey distinguishingFactor, Association association) {
			lock (this) {
				if (!this.serverAssocsTable.ContainsKey(distinguishingFactor)) {
					this.serverAssocsTable.Add(distinguishingFactor, new Associations());
				}
				Associations server_assocs = this.serverAssocsTable[distinguishingFactor];

				server_assocs.Set(association);
			}
		}

		/// <summary>
		/// Gets the best association (the one with the longest remaining life) for a given key.
		/// </summary>
		/// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for Providers).</param>
		/// <param name="securitySettings">The security settings.</param>
		/// <returns>
		/// The requested association, or null if no unexpired <see cref="Association"/>s exist for the given key.
		/// </returns>
		public Association GetAssociation(TKey distinguishingFactor, SecuritySettings securitySettings) {
			lock (this) {
				return this.GetServerAssociations(distinguishingFactor).Best.FirstOrDefault(assoc => securitySettings.IsAssociationInPermittedRange(assoc));
			}
		}

		/// <summary>
		/// Gets the association for a given key and handle.
		/// </summary>
		/// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for Providers).</param>
		/// <param name="handle">The handle of the specific association that must be recalled.</param>
		/// <returns>
		/// The requested association, or null if no unexpired <see cref="Association"/>s exist for the given key and handle.
		/// </returns>
		public Association GetAssociation(TKey distinguishingFactor, string handle) {
			lock (this) {
				return this.GetServerAssociations(distinguishingFactor).Get(handle);
			}
		}

		/// <summary>
		/// Removes a specified handle that may exist in the store.
		/// </summary>
		/// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for Providers).</param>
		/// <param name="handle">The handle of the specific association that must be deleted.</param>
		/// <returns>
		/// True if the association existed in this store previous to this call.
		/// </returns>
		/// <remarks>
		/// No exception should be thrown if the association does not exist in the store
		/// before this call.
		/// </remarks>
		public bool RemoveAssociation(TKey distinguishingFactor, string handle) {
			lock (this) {
				return this.GetServerAssociations(distinguishingFactor).Remove(handle);
			}
		}

		/// <summary>
		/// Clears all expired associations from the store.
		/// </summary>
		public void ClearExpiredAssociations() {
			lock (this) {
				foreach (Associations assocs in this.serverAssocsTable.Values) {
					assocs.ClearExpired();
				}
			}
		}

		/// <summary>
		/// Gets the server associations for a given OP Endpoint or dumb/smart mode.
		/// </summary>
		/// <param name="distinguishingFactor">The distinguishing factor, either an OP Endpoint (for relying parties) or smart/dumb (for providers).</param>
		/// <returns>The collection of associations that fit the <paramref name="distinguishingFactor"/>.</returns>
		internal Associations GetServerAssociations(TKey distinguishingFactor) {
			lock (this) {
				if (!this.serverAssocsTable.ContainsKey(distinguishingFactor)) {
					this.serverAssocsTable.Add(distinguishingFactor, new Associations());
				}

				return this.serverAssocsTable[distinguishingFactor];
			}
		}
	}
}
