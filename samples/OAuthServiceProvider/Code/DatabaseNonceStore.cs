namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// A database-persisted nonce store.
	/// </summary>
	public class DatabaseNonceStore : INonceStore {
		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseNonceStore"/> class.
		/// </summary>
		public DatabaseNonceStore() {
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
		/// property, accessible via the <see cref="DotNetOpenAuth.Configuration.DotNetOpenAuthSection.Configuration"/>
		/// property.
		/// </remarks>
		public bool StoreNonce(string context, string nonce, DateTime timestampUtc) {
			Global.DataContext.Nonces.InsertOnSubmit(new Nonce { Context = context, Code = nonce, Timestamp = timestampUtc });
			try {
				Global.DataContext.SubmitChanges();
				return true;
			} catch (System.Data.Linq.DuplicateKeyException) {
				return false;
			}
		}

		#endregion
	}
}