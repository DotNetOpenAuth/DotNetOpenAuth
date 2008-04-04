using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId;

namespace DotNetOpenId {
	public enum AssociationRelyingPartyType {
		Smart,
		Dumb
	}

	/// <summary>
	/// Stores <see cref="Association"/>s for lookup by their handle, keeping
	/// associations separated by a given distinguishing factor (like which server the
	/// association is with).
	/// </summary>
	/// <typeparam name="TKey">
	/// <see cref="System.Uri"/> for consumers (to distinguish associations across servers) or
	/// <see cref="AssociationRelyingPartyType"/> for providers (to distingish dumb and smart client associaitons).
	/// </typeparam>
	public interface IAssociationStore<TKey> {
		void StoreAssociation(TKey distinguishingFactor, Association assoc);
		/// <summary>
		/// Gets the best association (the one with the longest remaining life) for a given key.
		/// Null if no unexpired <see cref="Association"/>s exist for the given key.
		/// </summary>
		Association GetAssociation(TKey distinguishingFactor);
		/// <summary>
		/// Gets the association for a given key and handle.
		/// Null if no unexpired <see cref="Association"/>s exist for the given key and handle.
		/// </summary>
		Association GetAssociation(TKey distinguishingFactor, string handle);
		/// <summary>Removes a specified handle that may exist in the store.</summary>
		/// <returns>True if the association existed in this store previous to this call.</returns>
		/// <remarks>
		/// No exception should be thrown if the association does not exist in the store
		/// before this call.
		/// </remarks>
		bool RemoveAssociation(TKey distinguishingFactor, string handle);
		/// <summary>
		/// Clears all expired associations from the store.
		/// </summary>
		/// <remarks>
		/// If another algorithm is in place to periodically clear out expired associations,
		/// this method call may be ignored.
		/// This should be done frequently enough to avoid a memory leak, but sparingly enough
		/// to not be a performance drain.
		/// </remarks>
		void ClearExpiredAssociations();
	}
}
