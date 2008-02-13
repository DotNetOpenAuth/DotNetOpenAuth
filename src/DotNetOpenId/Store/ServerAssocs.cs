using System;
using System.Collections.Generic;

namespace DotNetOpenId.Store {
	public class ServerAssocs {

		Dictionary<string, Association> assocs;

		public ServerAssocs() {
			this.assocs = new Dictionary<string,Association>();
		}

		public void Set(Association assoc) {
			this.assocs[assoc.Handle] = assoc;
		}

		public Association Get(string handle) {
			Association assoc;
			assocs.TryGetValue(handle, out assoc);
			return assoc;
		}

		public bool Remove(string handle) {
			return assocs.Remove(handle);
		}

		public Association Best() {
			Association best = null;

			foreach (Association assoc in this.assocs.Values) {
				if (best == null || best.Issued < assoc.Issued)
					best = assoc;
			}

			return best;
		}

	}

}
