using System;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using DotNetOpenId;
using DotNetOpenId.RelyingParty;

namespace RelyingPartyCustomStore {
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
		public static CustomStore Instance = new CustomStore();
		public CustomStoreDataSet dataSet = new CustomStoreDataSet();

		#region IAssociationStore<Uri> Members

		public void StoreAssociation(Uri distinguishingFactor, Association assoc) {
			var assocRow = dataSet.Association.NewAssociationRow();
			assocRow.DistinguishingFactor = distinguishingFactor.AbsoluteUri;
			assocRow.Handle = assoc.Handle;
			assocRow.Expires = assoc.Expires.ToLocalTime();
			assocRow.PrivateData = assoc.SerializePrivateData();
			dataSet.Association.AddAssociationRow(assocRow);
		}

		public Association GetAssociation(Uri distinguishingFactor) {
			// properly escape the URL to prevent injection attacks.
			string value = distinguishingFactor.AbsoluteUri.Replace("'", "''");
			string filter = string.Format(CultureInfo.InvariantCulture, "{0} = '{1}'",
				dataSet.Association.DistinguishingFactorColumn.ColumnName, value);
			string sort = dataSet.Association.ExpiresColumn.ColumnName + " DESC";
			DataView view = new DataView(dataSet.Association, filter, sort, DataViewRowState.CurrentRows);
			if (view.Count == 0) return null;
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
			removeExpiredRows(dataSet.Association, dataSet.Association.ExpiresColumn.ColumnName);
		}

		#endregion

		#region INonceStore Members

		byte[] secretSigningKey;
		public byte[] SecretSigningKey {
			get {
				if (secretSigningKey == null) {
					lock (this) {
						if (secretSigningKey == null) {
							// initialize in a local variable before setting in field for thread safety.
							byte[] auth_key = new byte[64];
							new RNGCryptoServiceProvider().GetBytes(auth_key);
							this.secretSigningKey = auth_key;
						}
					}
				}
				return secretSigningKey;
			}
		}

		public bool TryStoreNonce(Nonce nonce) {
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
				if (dataSet.Nonce.FindByCode(nonce.Code) != null) return false;
				dataSet.Nonce.AddNonceRow(nonce.Code, nonce.ExpirationDate.ToLocalTime());
				return true;
			}
		}

		public void ClearExpiredNonces() {
			removeExpiredRows(dataSet.Nonce, dataSet.Nonce.ExpiresColumn.ColumnName);
		}

		#endregion

		void removeExpiredRows(DataTable table, string expiredColumnName) {
			string filter = string.Format(CultureInfo.InvariantCulture, "{0} < #{1}#",
				expiredColumnName, DateTime.Now);
			DataView view = new DataView(table, filter, null, DataViewRowState.CurrentRows);
			for (int i = view.Count - 1; i >= 0; i--)
				view.Delete(i);
		}

	}
}
