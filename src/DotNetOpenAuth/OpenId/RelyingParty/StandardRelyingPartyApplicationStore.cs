//-----------------------------------------------------------------------
// <copyright file="StandardRelyingPartyApplicationStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.ChannelElements;

	/// <summary>
	/// An in-memory store for Relying Parties, suitable for single server, single process
	/// ASP.NET web sites.
	/// </summary>
	public class StandardRelyingPartyApplicationStore : IRelyingPartyApplicationStore {
		/// <summary>
		/// The nonce store to use.
		/// </summary>
		private readonly INonceStore nonceStore;

		/// <summary>
		/// The association store to use.
		/// </summary>
		private readonly IAssociationStore<Uri> associationStore;

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardRelyingPartyApplicationStore"/> class.
		/// </summary>
		public StandardRelyingPartyApplicationStore() {
			this.nonceStore = new NonceMemoryStore(DotNetOpenAuthSection.Configuration.OpenId.MaxAuthenticationTime);
			this.associationStore = new AssociationMemoryStore<Uri>();
		}

		#region IAssociationStore<Uri> Members

		/// <summary>
		/// Saves an <see cref="Association"/> for later recall.
		/// </summary>
		/// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for providers).</param>
		/// <param name="association">The association to store.</param>
		public void StoreAssociation(Uri distinguishingFactor, Association association) {
			this.associationStore.StoreAssociation(distinguishingFactor, association);
		}

		/// <summary>
		/// Gets the best association (the one with the longest remaining life) for a given key.
		/// </summary>
		/// <param name="distinguishingFactor">The Uri (for relying parties) or Smart/Dumb (for Providers).</param>
		/// <param name="securityRequirements">The security settings.</param>
		/// <returns>
		/// The requested association, or null if no unexpired <see cref="Association"/>s exist for the given key.
		/// </returns>
		public Association GetAssociation(Uri distinguishingFactor, SecuritySettings securityRequirements) {
			return this.associationStore.GetAssociation(distinguishingFactor, securityRequirements);
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
			return this.associationStore.GetAssociation(distinguishingFactor, handle);
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
			return this.associationStore.RemoveAssociation(distinguishingFactor, handle);
		}

		#endregion

		#region INonceStore Members

		/// <summary>
		/// Stores a given nonce and timestamp.
		/// </summary>
		/// <param name="context">The context, or namespace, within which the <paramref name="nonce"/> must be unique.</param>
		/// <param name="nonce">A series of random characters.</param>
		/// <param name="timestampUtc">The timestamp that together with the nonce string make it unique.
		/// The timestamp may also be used by the data store to clear out old nonces.</param>
		/// <returns>
		/// True if the nonce+timestamp (combination) was not previously in the database.
		/// False if the nonce was stored previously with the same timestamp.
		/// </returns>
		/// <remarks>
		/// The nonce must be stored for no less than the maximum time window a message may
		/// be processed within before being discarded as an expired message.
		/// If the binding element is applicable to your channel, this expiration window
		/// is retrieved or set using the
		/// <see cref="StandardExpirationBindingElement.MaximumMessageAge"/> property.
		/// </remarks>
		public bool StoreNonce(string context, string nonce, DateTime timestampUtc) {
			return this.nonceStore.StoreNonce(context, nonce, timestampUtc);
		}

		#endregion
	}
}
