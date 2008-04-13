using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Yadis;
using System.IO;
using System.Xml;

namespace DotNetOpenId {
	class XriIdentifier : Identifier {
		internal static readonly char[] GlobalContextSymbols = { '=', '@', '+', '$', '!' };
		const string xriScheme = "xri://";

		public XriIdentifier(string xri) {
			if (!IsValidXri(xri))
				throw new FormatException(string.Format(CultureInfo.CurrentCulture,
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

		const string xriResolverProxy = "http://xri.net/{0}?_xrd_r=application/xrds%2Bxml;sep=false";
		/// <summary>
		/// Resolves the XRI to a URL from which an XRDS document may be downloaded.
		/// </summary>
		protected virtual Uri XrdsUrl {
			get {
				return new Uri(string.Format(CultureInfo.InvariantCulture, 
					xriResolverProxy, this));
			}
		}

		XrdsDocument downloadXrds() {
			var xrdsResponse = UntrustedWebRequest.Request(XrdsUrl);
			return new XrdsDocument(XmlReader.Create(xrdsResponse.ResponseStream));
		}

		internal override ServiceEndpoint Discover() {
			return downloadXrds().CreateServiceEndpoint(this);
		}

		public override bool Equals(object obj) {
			XriIdentifier other = obj as XriIdentifier;
			if (other == null) return false;
			return this.CanonicalXri == other.CanonicalXri;
		}
		public override int GetHashCode() {
			return CanonicalXri.GetHashCode();
		}

		public override string ToString() {
			return CanonicalXri;
		}
	}
}
