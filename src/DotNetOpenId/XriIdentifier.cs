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

		/// <summary>
		/// The magic URL that will provide us an XRDS document for a given XRI identifier.
		/// </summary>
		/// <remarks>
		/// We use application/xrd+xml instead of application/xrds+xml because it gets
		/// xri.net to automatically give us exactly the right XRD element for community i-names
		/// automatically, saving us having to choose which one to use out of the result.
		/// </remarks>
		const string xriResolverProxy = "https://xri.net/{0}?_xrd_r=application/xrd%2Bxml;sep=false";
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

		internal override IEnumerable<ServiceEndpoint> Discover() {
			return downloadXrds().CreateServiceEndpoints(this);
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
