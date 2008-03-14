using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace DotNetOpenId {
	class XriIdentifier : Identifier {
		internal static readonly char[] GlobalContextSymbols = { '=', '@', '+', '$', '!' };
		const string xriScheme = "xri://";

		public XriIdentifier(string xri) {
			if (!IsValidXri(xri))
				throw new FormatException(string.Format(CultureInfo.CurrentUICulture,
					Strings.InvalidXri, xri));
			OriginalXri = xri;
			CanonicalXri = canonicalizeXri(xri);
		}

		/// <summary>
		/// The original XRI supplied to the constructor.
		/// </summary>
		public string OriginalXri { get; private set; }
		/// <summary>
		/// The canonical form of the XRI string.
		/// </summary>
		public string CanonicalXri { get; private set; }

		/// <summary>
		/// Tests whether a given string represents a valid XRI format.
		/// </summary>
		internal static bool IsValidXri(string xri) {
			if (string.IsNullOrEmpty(xri)) throw new ArgumentNullException("xri");
			// TODO: better validation code here
			return xri.IndexOfAny(GlobalContextSymbols) == 0
				|| xri.StartsWith("(", StringComparison.Ordinal)
				|| xri.StartsWith(xriScheme, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Takes any valid form of XRI string and returns the canonical form of the same XRI.
		/// </summary>
		static string canonicalizeXri(string xri) {
			if (xri.StartsWith(xriScheme, StringComparison.OrdinalIgnoreCase))
				xri = xri.Substring(xriScheme.Length);
			return xri;
		}

		public override bool Equals(object obj) {
			XriIdentifier other = obj as XriIdentifier;
			if (other == null) return false;
			return this.CanonicalXri == other.CanonicalXri;
		}

		public override string ToString() {
			return CanonicalXri;
		}
	}
}
