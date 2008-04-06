using System;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using DotNetOpenId;
using DotNetOpenId.RelyingParty;
using IProviderAssociationStore = DotNetOpenId.IAssociationStore<DotNetOpenId.AssociationRelyingPartyType>;

namespace ProviderCustomStore {
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
	public class CustomStore : IProviderAssociationStore {
		public static CustomStore Instance = new CustomStore();
		public CustomStoreDataSet dataSet = new CustomStoreDataSet();

		#region IAssociationStore<AssociationRelyingPartyType> Members

		public void StoreAssociation(AssociationRelyingPartyType distinguishingFactor, Association assoc) {
			var assocRow = dataSet.Association.NewAssociationRow();
			assocRow.DistinguishingFactor = distinguishingFactor.ToString();
			assocRow.Handle = assoc.Handle;
			assocRow.Expires = assoc.Expires.ToLocalTime();
			assocRow.PrivateData = assoc.SerializePrivateData();
			dataSet.Association.AddAssociationRow(assocRow);
		}

		public Association GetAssociation(AssociationRelyingPartyType distinguishingFactor) {
			// properly escape the URL to prevent injection attacks.
			string value = distinguishingFactor.ToString();
			string filter = string.Format(CultureInfo.InvariantCulture, "{0} = '{1}'",
				dataSet.Association.DistinguishingFactorColumn.ColumnName, value);
			string sort = dataSet.Association.ExpiresColumn.ColumnName + " DESC";
			DataView view = new DataView(dataSet.Association, filter, sort, DataViewRowState.CurrentRows);
			if (view.Count == 0) return null;
			var row = (CustomStoreDataSet.AssociationRow)view[0].Row;
			return Association.Deserialize(row.Handle, row.Expires.ToUniversalTime(), row.PrivateData);
		}

		public Association GetAssociation(AssociationRelyingPartyType distinguishingFactor, string handle) {
			var assocRow = dataSet.Association.FindByDistinguishingFactorHandle(distinguishingFactor.ToString(), handle);
			return Association.Deserialize(assocRow.Handle, assocRow.Expires, assocRow.PrivateData);
		}

		public bool RemoveAssociation(AssociationRelyingPartyType distinguishingFactor, string handle) {
			var row = dataSet.Association.FindByDistinguishingFactorHandle(distinguishingFactor.ToString(), handle);
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

		void removeExpiredRows(DataTable table, string expiredColumnName) {
			string filter = string.Format(CultureInfo.InvariantCulture, "{0} < #{1}#",
				expiredColumnName, DateTime.Now);
			DataView view = new DataView(table, filter, null, DataViewRowState.CurrentRows);
			for (int i = view.Count - 1; i >= 0; i--)
				view.Delete(i);
		}

	}
}
