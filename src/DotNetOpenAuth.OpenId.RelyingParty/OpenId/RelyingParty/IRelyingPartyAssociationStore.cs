//-----------------------------------------------------------------------
// <copyright file="IRelyingPartyAssociationStore.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using Validation;

	/// <summary>
	/// Stores <see cref="Association"/>s for lookup by their handle, keeping
	/// associations separated by a given OP Endpoint.
	/// </summary>
	/// <remarks>
	/// Expired associations should be periodically cleared out of an association store.
	/// This should be done frequently enough to avoid a memory leak, but sparingly enough
	/// to not be a performance drain.  Because this balance can vary by host, it is the
	/// responsibility of the host to initiate this cleaning.
	/// </remarks>
	public interface IRelyingPartyAssociationStore {
		/// <summary>
		/// Saves an <see cref="Association"/> for later recall.
		/// </summary>
		/// <param name="providerEndpoint">The OP Endpoint with which the association is established.</param>
		/// <param name="association">The association to store.</param>
		/// <remarks>
		/// If the new association conflicts (in OP endpoint and association handle) with an existing association,
		/// (which should never happen by the way) implementations may overwrite the previously saved association.
		/// </remarks>
		void StoreAssociation(Uri providerEndpoint, Association association);

		/// <summary>
		/// Gets the best association (the one with the longest remaining life) for a given key.
		/// </summary>
		/// <param name="providerEndpoint">The OP Endpoint with which the association is established.</param>
		/// <param name="securityRequirements">The security requirements that the returned association must meet.</param>
		/// <returns>
		/// The requested association, or null if no unexpired <see cref="Association"/>s exist for the given key.
		/// </returns>
		/// <remarks>
		/// In the event that multiple associations exist for the given 
		/// <paramref name="providerEndpoint"/>, it is important for the 
		/// implementation for this method to use the <paramref name="securityRequirements"/>
		/// to pick the best (highest grade or longest living as the host's policy may dictate)
		/// association that fits the security requirements.
		/// Associations that are returned that do not meet the security requirements will be
		/// ignored and a new association created.
		/// </remarks>
		Association GetAssociation(Uri providerEndpoint, SecuritySettings securityRequirements);

		/// <summary>
		/// Gets the association for a given key and handle.
		/// </summary>
		/// <param name="providerEndpoint">The OP Endpoint with which the association is established.</param>
		/// <param name="handle">The handle of the specific association that must be recalled.</param>
		/// <returns>The requested association, or null if no unexpired <see cref="Association"/>s exist for the given key and handle.</returns>
		Association GetAssociation(Uri providerEndpoint, string handle);

		/// <summary>Removes a specified handle that may exist in the store.</summary>
		/// <param name="providerEndpoint">The OP Endpoint with which the association is established.</param>
		/// <param name="handle">The handle of the specific association that must be deleted.</param>
		/// <returns>
		/// Deprecated.  The return value is insignificant.
		/// Previously: True if the association existed in this store previous to this call.
		/// </returns>
		/// <remarks>
		/// No exception should be thrown if the association does not exist in the store
		/// before this call.
		/// </remarks>
		bool RemoveAssociation(Uri providerEndpoint, string handle);
	}
}
