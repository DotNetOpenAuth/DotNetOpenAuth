//-----------------------------------------------------------------------
// <copyright file="DuplicateRequestedHostsComparer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// An authentication request comparer that judges equality solely on the OP endpoint hostname.
	/// </summary>
	internal class DuplicateRequestedHostsComparer : IEqualityComparer<IAuthenticationRequest> {
		/// <summary>
		/// The singleton instance of this comparer.
		/// </summary>
		private static IEqualityComparer<IAuthenticationRequest> instance = new DuplicateRequestedHostsComparer();

		/// <summary>
		/// Prevents a default instance of the <see cref="DuplicateRequestedHostsComparer"/> class from being created.
		/// </summary>
		private DuplicateRequestedHostsComparer() {
		}

		/// <summary>
		/// Gets the singleton instance of this comparer.
		/// </summary>
		internal static IEqualityComparer<IAuthenticationRequest> Instance {
			get { return instance; }
		}

		#region IEqualityComparer<IAuthenticationRequest> Members

		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		/// true if the specified objects are equal; otherwise, false.
		/// </returns>
		public bool Equals(IAuthenticationRequest x, IAuthenticationRequest y) {
			if (x == null && y == null) {
				return true;
			}

			if (x == null || y == null) {
				return false;
			}

			// We'll distinguish based on the host name only, which
			// admittedly is only a heuristic, but if we remove one that really wasn't a duplicate, well,
			// this multiple OP attempt thing was just a convenience feature anyway.
			return string.Equals(x.Provider.Uri.Host, y.Provider.Uri.Host, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Returns a hash code for the specified object.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
		/// <returns>A hash code for the specified object.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.
		/// </exception>
		public int GetHashCode(IAuthenticationRequest obj) {
			return obj.Provider.Uri.Host.GetHashCode();
		}

		#endregion
	}
}
