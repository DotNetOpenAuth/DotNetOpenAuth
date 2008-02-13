using System;
using System.Collections.Generic;

namespace DotNetOpenId.Store {
	internal class ServerAssocs {

		Dictionary<string, Association> assocs;

		/// <summary>
		/// Instantiates a mapping between association handles and <see cref="Association"/> objects.
		/// </summary>
		public ServerAssocs() {
			this.assocs = new Dictionary<string,Association>();
		}

		/// <summary>
		/// Stores an <see cref="Association"/> in the collection.
		/// </summary>
		/// <param name="assoc"></param>
		public void Set(Association assoc) {
			this.assocs[assoc.Handle] = assoc;
		}

		/// <summary>
		/// Returns the <see cref="Association"/> with the given handle.
		/// </summary>
		public Association Get(string handle) {
			Association assoc;
			assocs.TryGetValue(handle, out assoc);
			return assoc;
		}

		/// <summary>
		/// Removes the <see cref="Association"/> with the given handle.
		/// </summary>
		/// <returns>Whether an <see cref="Association"/> with the given handle was in the collection for removal.</returns>
		public bool Remove(string handle) {
			return assocs.Remove(handle);
		}

		/// <summary>
		/// Gets the <see cref="Association"/> issued most recently.
		/// </summary>
		public Association Best {
			get {
				Association best = null;

				foreach (Association assoc in this.assocs.Values) {
					if (best == null || best.Issued < assoc.Issued)
						best = assoc;
				}

				return best;
			}
		}

	}

}
