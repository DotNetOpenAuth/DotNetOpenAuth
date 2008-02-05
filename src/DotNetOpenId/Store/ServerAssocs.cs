using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DotNetOpenId.Store {
	public class ServerAssocs {

		#region Private Members

		private Hashtable assocs;

		#endregion

		#region Constructor(s)

		public ServerAssocs() {
			this.assocs = new Hashtable();
		}

		#endregion

		#region Methods

		public void Set(Association assoc) {
			this.assocs[assoc.Handle] = assoc;
		}

		public Association Get(string handle) {
			Association assoc = null;

			if (this.assocs.Contains(handle))
				assoc = (Association)this.assocs[handle];

			return assoc;
		}

		public bool Remove(string handle) {
			bool present = this.assocs.Contains(handle);

			this.assocs.Remove(handle);

			return present;
		}

		public Association Best() {
			Association best = null;

			foreach (Association assoc in this.assocs.Values) {
				if (best == null || best.Issued < assoc.Issued)
					best = assoc;
			}

			return best;
		}

		#endregion

	}

}
