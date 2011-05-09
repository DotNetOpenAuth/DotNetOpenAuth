//-----------------------------------------------------------------------
// <copyright file="RelyingPartyApplicationDbStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// A database-backed state store for OpenID relying parties.
	/// </summary>
	public class RelyingPartyApplicationDbStore : NonceDbStore, IRelyingPartyApplicationStore {
		/// <summary>
		/// Initializes a new instance of the <see cref="RelyingPartyApplicationDbStore"/> class.
		/// </summary>
		public RelyingPartyApplicationDbStore() {
		}

		#region IRelyingPartyApplicationStore Members

		/// <summary>
		/// Saves an <see cref="Association"/> for later recall.
		/// </summary>
		/// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for providers).</param>
		/// <param name="association">The association to store.</param>
		/// <remarks>
		/// TODO: what should implementations do on association handle conflict?
		/// </remarks>
		public void StoreAssociation(Uri distinguishingFactor, Association association) {
			using (var dataContext = new TransactedDatabaseEntities(System.Data.IsolationLevel.ReadCommitted)) {
				var sharedAssociation = new OpenIdAssociation {
					DistinguishingFactor = distinguishingFactor.AbsoluteUri,
					AssociationHandle = association.Handle,
					ExpirationUtc = association.Expires,
					PrivateData = association.SerializePrivateData(),
				};

				dataContext.AddToOpenIdAssociations(sharedAssociation);
			}
		}

		/// <summary>
		/// Gets the best association (the one with the longest remaining life) for a given key.
		/// </summary>
		/// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for Providers).</param>
		/// <param name="securityRequirements">The security requirements that the returned association must meet.</param>
		/// <returns>
		/// The requested association, or null if no unexpired <see cref="Association"/>s exist for the given key.
		/// </returns>
		/// <remarks>
		/// In the event that multiple associations exist for the given
		/// <paramref name="distinguishingFactor"/>, it is important for the
		/// implementation for this method to use the <paramref name="securityRequirements"/>
		/// to pick the best (highest grade or longest living as the host's policy may dictate)
		/// association that fits the security requirements.
		/// Associations that are returned that do not meet the security requirements will be
		/// ignored and a new association created.
		/// </remarks>
		public Association GetAssociation(Uri distinguishingFactor, SecuritySettings securityRequirements) {
			using (var dataContext = new TransactedDatabaseEntities(System.Data.IsolationLevel.ReadCommitted)) {
				var relevantAssociations = from assoc in dataContext.OpenIdAssociations
										   where assoc.DistinguishingFactor == distinguishingFactor.AbsoluteUri
										   where assoc.ExpirationUtc > DateTime.UtcNow
										   where assoc.PrivateDataLength * 8 >= securityRequirements.MinimumHashBitLength
										   where assoc.PrivateDataLength * 8 <= securityRequirements.MaximumHashBitLength
										   orderby assoc.ExpirationUtc descending
										   select assoc;
				var qualifyingAssociations = relevantAssociations.AsEnumerable()
					.Select(assoc => DeserializeAssociation(assoc));
				return qualifyingAssociations.FirstOrDefault();
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
		public Association GetAssociation(Uri distinguishingFactor, string handle) {
			using (var dataContext = new TransactedDatabaseEntities(System.Data.IsolationLevel.ReadCommitted)) {
				var associations = from assoc in dataContext.OpenIdAssociations
								   where assoc.DistinguishingFactor == distinguishingFactor.AbsoluteUri
								   where assoc.AssociationHandle == handle
								   where assoc.ExpirationUtc > DateTime.UtcNow
								   select assoc;
				return associations.AsEnumerable()
					.Select(assoc => DeserializeAssociation(assoc))
					.FirstOrDefault();
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
		public bool RemoveAssociation(Uri distinguishingFactor, string handle) {
			using (var dataContext = new TransactedDatabaseEntities(System.Data.IsolationLevel.ReadCommitted)) {
				var association = dataContext.OpenIdAssociations.FirstOrDefault(a => a.DistinguishingFactor == distinguishingFactor.AbsoluteUri && a.AssociationHandle == handle);
				if (association != null) {
					dataContext.DeleteObject(association);
					return true;
				} else {
					return false;
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
		internal void ClearExpiredAssociations() {
			using (var dataContext = new TransactedDatabaseEntities(IsolationLevel.ReadCommitted)) {
				dataContext.ClearExpiredAssociations(dataContext.Transaction);
			}
		}

		/// <summary>
		/// Deserializes an association from the database.
		/// </summary>
		/// <param name="association">The association from the database.</param>
		/// <returns>The deserialized association.</returns>
		private static Association DeserializeAssociation(OpenIdAssociation association) {
			if (association == null) {
				throw new ArgumentNullException("association");
			}

			byte[] privateData = new byte[association.PrivateDataLength];
			Array.Copy(association.PrivateData, privateData, association.PrivateDataLength);
			return Association.Deserialize(association.AssociationHandle, association.ExpirationUtc, privateData);
		}
	}
}
