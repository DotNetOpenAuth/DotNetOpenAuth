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
	/// <see cref="AssociationConsumerType"/> for providers (to distingish dumb and smart client associaitons).
	/// </typeparam>
	public interface IAssociationStore<TKey> {
		void StoreAssociation(TKey distinguishingFactor, Association assoc);
		Association GetAssociation(TKey distinguishingFactor);
		Association GetAssociation(TKey distinguishingFactor, string handle);
		bool RemoveAssociation(TKey distinguishingFactor, string handle);
		/// <summary>
		/// Clears all expired associations from the store.
		/// </summary>
		/// <remarks>
		/// This should be done frequently enough to avoid a memory leak, but sparingly enough
		/// to not be a performance drain.
		/// </remarks>
		void ClearExpiredAssociations();
	}
}
