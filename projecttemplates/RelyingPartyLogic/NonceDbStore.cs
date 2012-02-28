//-----------------------------------------------------------------------
// <copyright file="NonceDbStore.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using System.Data.EntityClient;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// A database-backed nonce store for OpenID and OAuth services.
	/// </summary>
	public class NonceDbStore : INonceStore {
		private const int NonceClearingInterval = 5;

		/// <summary>
		/// A counter that tracks how many nonce stores have been done.
		/// </summary>
		private static int nonceClearingCounter;

		/// <summary>
		/// Initializes a new instance of the <see cref="NonceDbStore"/> class.
		/// </summary>
		public NonceDbStore() {
		}

		#region INonceStore Members

		/// <summary>
		/// Stores a given nonce and timestamp.
		/// </summary>
		/// <param name="context">The context, or namespace, within which the
		/// <paramref name="nonce"/> must be unique.
		/// The context SHOULD be treated as case-sensitive.
		/// The value will never be <c>null</c> but may be the empty string.</param>
		/// <param name="nonce">A series of random characters.</param>
		/// <param name="timestampUtc">The UTC timestamp that together with the nonce string make it unique
		/// within the given <paramref name="context"/>.
		/// The timestamp may also be used by the data store to clear out old nonces.</param>
		/// <returns>
		/// True if the context+nonce+timestamp (combination) was not previously in the database.
		/// False if the nonce was stored previously with the same timestamp and context.
		/// </returns>
		/// <remarks>
		/// The nonce must be stored for no less than the maximum time window a message may
		/// be processed within before being discarded as an expired message.
		/// This maximum message age can be looked up via the
		/// <see cref="DotNetOpenAuth.Configuration.MessagingElement.MaximumMessageLifetime"/>
		/// property, accessible via the <see cref="DotNetOpenAuth.Configuration.MessagingElement.Configuration"/>
		/// property.
		/// </remarks>
		public bool StoreNonce(string context, string nonce, DateTime timestampUtc) {
			try {
				using (var dataContext = new TransactedDatabaseEntities(IsolationLevel.ReadCommitted)) {
					Nonce nonceEntity = new Nonce {
						Context = context,
						Code = nonce,
						IssuedUtc = timestampUtc,
						ExpiresUtc = timestampUtc + DotNetOpenAuthSection.Messaging.MaximumMessageLifetime,
					};

					// The database columns [context] and [code] MUST be using
					// a case sensitive collation for this to be secure.
					dataContext.AddToNonces(nonceEntity);
				}
			} catch (UpdateException) {
				// A nonce collision
				return false;
			}

			// Only clear nonces after successfully storing a nonce.
			// This mitigates cheap DoS attacks that take up a lot of
			// database cycles.
			ClearNoncesIfAppropriate();
			return true;
		}

		#endregion

		/// <summary>
		/// Clears the nonces if appropriate.
		/// </summary>
		private static void ClearNoncesIfAppropriate() {
			if (++nonceClearingCounter % NonceClearingInterval == 0) {
				using (var dataContext = new TransactedDatabaseEntities(IsolationLevel.ReadCommitted)) {
					dataContext.ClearExpiredNonces(dataContext.Transaction);
				}
			}
		}

		/// <summary>
		/// A transacted data context.
		/// </summary>
		protected class TransactedDatabaseEntities : DatabaseEntities {
			/// <summary>
			/// Initializes a new instance of the <see cref="TransactedDatabaseEntities"/> class.
			/// </summary>
			/// <param name="isolationLevel">The isolation level.</param>
			public TransactedDatabaseEntities(IsolationLevel isolationLevel) {
				this.Connection.Open();
				this.Transaction = (EntityTransaction)this.Connection.BeginTransaction(isolationLevel);
			}

			/// <summary>
			/// Gets the transaction for this data context.
			/// </summary>
			public EntityTransaction Transaction { get; private set; }

			/// <summary>
			/// Releases the resources used by the object context.
			/// </summary>
			/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
			protected override void Dispose(bool disposing) {
				try {
					this.SaveChanges();
					this.Transaction.Commit();
				} finally {
					this.Connection.Close();
				}

				base.Dispose(disposing);
			}
		}
	}
}
