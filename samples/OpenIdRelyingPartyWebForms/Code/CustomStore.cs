namespace OpenIdRelyingPartyWebForms.Code {
	using System;
	using System.Data;
	using System.Globalization;
	using System.Security.Cryptography;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// This custom store serializes all elements to demonstrate peristent and/or shared storage.
	/// This is common in a web farm, for example.
	/// </summary>
	/// <remarks>
	/// This doesn't actually serialize anything to a persistent store, so restarting the web server
	/// will still clear everything this store is supposed to remember.
	/// But we "persist" all associations and nonces into a DataTable to demonstrate
	/// that using a database is possible.
	/// </remarks>
	public class CustomStore : IRelyingPartyApplicationStore {
		private static CustomStoreDataSet dataSet = new CustomStoreDataSet();

		#region INonceStore Members

		/// <summary>
		/// Stores a given nonce and timestamp.
		/// </summary>
		/// <param name="context">The context, or namespace, within which the
		/// <paramref name="nonce"/> must be unique.
		/// The context SHOULD be treated as case-sensitive.
		/// The value will never be <c>null</c> but may be the empty string.</param>
		/// <param name="nonce">A series of random characters.</param>
		/// <param name="timestamp">The timestamp that together with the nonce string make it unique.
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
		public bool StoreNonce(string context, string nonce, DateTime timestamp) {
			// IMPORTANT: If actually persisting to a database that can be reached from
			// different servers/instances of this class at once, it is vitally important
			// to protect against race condition attacks by one or more of these:
			// 1) setting a UNIQUE constraint on the nonce CODE in the SQL table
			// 2) Using a transaction with repeatable reads to guarantee that a check
			//    that verified a nonce did not exist will prevent that nonce from being
			//    added by another process while this process is adding it.
			// And then you'll want to catch the exception that the SQL database can throw
			// at you in the result of a race condition somewhere in your web site UI code
			// and display some message to have the user try to log in again, and possibly
			// warn them about a replay attack.
			timestamp = timestamp.ToLocalTime();
			lock (this) {
				if (dataSet.Nonce.FindByIssuedCodeContext(timestamp, nonce, context) != null) {
					return false;
				}

				TimeSpan maxMessageAge = DotNetOpenAuth.Configuration.DotNetOpenAuthSection.Configuration.Messaging.MaximumMessageLifetime;
				dataSet.Nonce.AddNonceRow(context, nonce, timestamp, timestamp + maxMessageAge);
				return true;
			}
		}

		public void ClearExpiredNonces() {
			this.removeExpiredRows(dataSet.Nonce, dataSet.Nonce.ExpiresColumn.ColumnName);
		}

		#endregion

		#region IAssociationStore<Uri> Members

		public void StoreAssociation(Uri distinguishingFactor, Association assoc) {
			var assocRow = dataSet.Association.NewAssociationRow();
			assocRow.DistinguishingFactor = distinguishingFactor.AbsoluteUri;
			assocRow.Handle = assoc.Handle;
			assocRow.Expires = assoc.Expires.ToLocalTime();
			assocRow.PrivateData = assoc.SerializePrivateData();
			dataSet.Association.AddAssociationRow(assocRow);
		}

		public Association GetAssociation(Uri distinguishingFactor, SecuritySettings securitySettings) {
			// TODO: properly consider the securitySettings when picking an association to return.
			// properly escape the URL to prevent injection attacks.
			string value = distinguishingFactor.AbsoluteUri.Replace("'", "''");
			string filter = string.Format(
				CultureInfo.InvariantCulture,
				"{0} = '{1}'",
				dataSet.Association.DistinguishingFactorColumn.ColumnName,
				value);
			string sort = dataSet.Association.ExpiresColumn.ColumnName + " DESC";
			DataView view = new DataView(dataSet.Association, filter, sort, DataViewRowState.CurrentRows);
			if (view.Count == 0) {
				return null;
			}
			var row = (CustomStoreDataSet.AssociationRow)view[0].Row;
			return Association.Deserialize(row.Handle, row.Expires.ToUniversalTime(), row.PrivateData);
		}

		public Association GetAssociation(Uri distinguishingFactor, string handle) {
			var assocRow = dataSet.Association.FindByDistinguishingFactorHandle(distinguishingFactor.AbsoluteUri, handle);
			return Association.Deserialize(assocRow.Handle, assocRow.Expires, assocRow.PrivateData);
		}

		public bool RemoveAssociation(Uri distinguishingFactor, string handle) {
			var row = dataSet.Association.FindByDistinguishingFactorHandle(distinguishingFactor.AbsoluteUri, handle);
			if (row != null) {
				dataSet.Association.RemoveAssociationRow(row);
				return true;
			} else {
				return false;
			}
		}

		public void ClearExpiredAssociations() {
			this.removeExpiredRows(dataSet.Association, dataSet.Association.ExpiresColumn.ColumnName);
		}

		#endregion

		private void removeExpiredRows(DataTable table, string expiredColumnName) {
			string filter = string.Format(CultureInfo.InvariantCulture, "{0} < #{1}#", expiredColumnName, DateTime.Now);
			DataView view = new DataView(table, filter, null, DataViewRowState.CurrentRows);
			for (int i = view.Count - 1; i >= 0; i--) {
				view.Delete(i);
			}
		}
	}
}
