//-----------------------------------------------------------------------
// <copyright file="CustomStore.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OpenIdProviderWebForms.Code {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using DotNetOpenAuth;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;

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
	public class CustomStore : ICryptoKeyAndNonceStore {
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
			lock (this) {
				if (dataSet.Nonce.FindByIssuedUtcCodeContext(timestampUtc, nonce, context) != null) {
					return false;
				}

				TimeSpan maxMessageAge = DotNetOpenAuthSection.Messaging.MaximumMessageLifetime;
				dataSet.Nonce.AddNonceRow(context, nonce, timestampUtc, timestampUtc + maxMessageAge);
				return true;
			}
		}

		public void ClearExpiredNonces() {
			this.removeExpiredRows(dataSet.Nonce, dataSet.Nonce.ExpiresUtcColumn.ColumnName);
		}

		#endregion

		#region ICryptoKeyStore Members

		public CryptoKey GetKey(string bucket, string handle) {
			var assocRow = dataSet.CryptoKey.FindByBucketHandle(bucket, handle);
			return new CryptoKey(assocRow.Secret, assocRow.ExpiresUtc);
		}

		public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket) {
			// properly escape the URL to prevent injection attacks.
			string value = bucket.Replace("'", "''");
			string filter = string.Format(
				CultureInfo.InvariantCulture,
				"{0} = '{1}'",
				dataSet.CryptoKey.BucketColumn.ColumnName,
				value);
			string sort = dataSet.CryptoKey.ExpiresUtcColumn.ColumnName + " DESC";
			DataView view = new DataView(dataSet.CryptoKey, filter, sort, DataViewRowState.CurrentRows);
			if (view.Count == 0) {
				yield break;
			}

			foreach (CustomStoreDataSet.CryptoKeyRow row in view.Cast<DataRowView>().Select(rv => rv.Row)) {
				yield return new KeyValuePair<string, CryptoKey>(row.Handle, new CryptoKey(row.Secret, row.ExpiresUtc));
			}
		}

		public void StoreKey(string bucket, string handle, CryptoKey key) {
			var cryptoKeyRow = dataSet.CryptoKey.NewCryptoKeyRow();
			cryptoKeyRow.Bucket = bucket;
			cryptoKeyRow.Handle = handle;
			cryptoKeyRow.ExpiresUtc = key.ExpiresUtc;
			cryptoKeyRow.Secret = key.Key;
			dataSet.CryptoKey.AddCryptoKeyRow(cryptoKeyRow);
		}

		public void RemoveKey(string bucket, string handle) {
			var row = dataSet.CryptoKey.FindByBucketHandle(bucket, handle);
			if (row != null) {
				dataSet.CryptoKey.RemoveCryptoKeyRow(row);
			}
		}

		#endregion

		internal void ClearExpiredSecrets() {
			this.removeExpiredRows(dataSet.CryptoKey, dataSet.CryptoKey.ExpiresUtcColumn.ColumnName);
		}

		private void removeExpiredRows(DataTable table, string expiredColumnName) {
			string filter = string.Format(CultureInfo.InvariantCulture, "{0} < #{1}#", expiredColumnName, DateTime.UtcNow);
			DataView view = new DataView(table, filter, null, DataViewRowState.CurrentRows);
			for (int i = view.Count - 1; i >= 0; i--) {
				view.Delete(i);
			}
		}
	}
}
