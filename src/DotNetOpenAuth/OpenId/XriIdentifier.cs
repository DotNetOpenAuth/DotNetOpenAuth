//-----------------------------------------------------------------------
// <copyright file="XriIdentifier.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Xrds;
	using DotNetOpenAuth.Yadis;

	/// <summary>
	/// An XRI style of OpenID Identifier.
	/// </summary>
	[Serializable]
	public sealed class XriIdentifier : Identifier {
		/// <summary>
		/// An XRI always starts with one of these symbols.
		/// </summary>
		internal static readonly char[] GlobalContextSymbols = { '=', '@', '+', '$', '!' };

		/// <summary>
		/// The scheme and separator "xri://"
		/// </summary>
		private const string XriScheme = "xri://";

		/// <summary>
		/// The magic URL that will provide us an XRDS document for a given XRI identifier.
		/// </summary>
		/// <remarks>
		/// We use application/xrd+xml instead of application/xrds+xml because it gets
		/// xri.net to automatically give us exactly the right XRD element for community i-names
		/// automatically, saving us having to choose which one to use out of the result.
		/// The ssl=true parameter tells the proxy resolver to accept only SSL connections
		/// when resolving community i-names.
		/// </remarks>
		private const string XriResolverProxyTemplate = "https://xri.net/{0}?_xrd_r=application/xrd%2Bxml;sep=false";

		/// <summary>
		/// The XRI proxy resolver to use for finding XRDS documents from an XRI.
		/// </summary>
		private readonly string xriResolverProxy;

		/// <summary>
		/// Initializes a new instance of the <see cref="XriIdentifier"/> class.
		/// </summary>
		/// <param name="xri">The string value of the XRI.</param>
		internal XriIdentifier(string xri)
			: this(xri, false) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XriIdentifier"/> class.
		/// </summary>
		/// <param name="xri">The XRI that this Identifier will represent.</param>
		/// <param name="requireSsl">
		/// If set to <c>true</c>, discovery and the initial authentication redirect will
		/// only succeed if it can be done entirely using SSL.
		/// </param>
		internal XriIdentifier(string xri, bool requireSsl)
			: base(requireSsl) {
			if (!IsValidXri(xri)) {
				throw new FormatException(
					string.Format(CultureInfo.CurrentCulture, OpenIdStrings.InvalidXri, xri));
			}
			this.xriResolverProxy = XriResolverProxyTemplate;
			if (requireSsl) {
				// Indicate to xri.net that we require SSL to be used for delegated resolution
				// of community i-names.
				this.xriResolverProxy += ";https=true";
			}
			this.OriginalXri = xri;
			this.CanonicalXri = CanonicalizeXri(xri);
		}

		/// <summary>
		/// Gets the original XRI supplied to the constructor.
		/// </summary>
		internal string OriginalXri { get; private set; }

		/// <summary>
		/// Gets the canonical form of the XRI string.
		/// </summary>
		internal string CanonicalXri { get; private set; }

		/// <summary>
		/// Gets the URL from which this XRI's XRDS document may be downloaded.
		/// </summary>
		private Uri XrdsUrl {
			get { return new Uri(string.Format(CultureInfo.InvariantCulture, this.xriResolverProxy, this)); }
		}

		/// <summary>
		/// Tests equality between this XRI and another XRI.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			XriIdentifier other = obj as XriIdentifier;
			if (other == null) {
				return false;
			}
			return this.CanonicalXri == other.CanonicalXri;
		}

		/// <summary>
		/// Returns the hash code of this XRI.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			return this.CanonicalXri.GetHashCode();
		}

		/// <summary>
		/// Returns the canonical string form of the XRI.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString() {
			return this.CanonicalXri;
		}

		/// <summary>
		/// Tests whether a given string represents a valid XRI format.
		/// </summary>
		/// <param name="xri">The value to test for XRI validity.</param>
		/// <returns>
		/// 	<c>true</c> if the given string constitutes a valid XRI; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsValidXri(string xri) {
			ErrorUtilities.VerifyNonZeroLength(xri, "xri");
			xri = xri.Trim();

			// TODO: better validation code here
			return xri.IndexOfAny(GlobalContextSymbols) == 0
				|| xri.StartsWith("(", StringComparison.Ordinal)
				|| xri.StartsWith(XriScheme, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Performs discovery on the Identifier.
		/// </summary>
		/// <param name="requestHandler">The web request handler to use for discovery.</param>
		/// <returns>
		/// An initialized structure containing the discovered provider endpoint information.
		/// </returns>
		internal override IEnumerable<ServiceEndpoint> Discover(IDirectWebRequestHandler requestHandler) {
			return this.DownloadXrds(requestHandler).CreateServiceEndpoints(this);
		}

		/// <summary>
		/// Performs discovery on THIS identifier, but generates <see cref="ServiceEndpoint"/>
		/// instances that treat another given identifier as the user-supplied identifier.
		/// </summary>
		/// <param name="requestHandler">The request handler to use in discovery.</param>
		/// <param name="userSuppliedIdentifier">The user supplied identifier, which may differ from this XRI instance due to multiple discovery steps.</param>
		/// <returns>A list of service endpoints offered for this identifier.</returns>
		internal IEnumerable<ServiceEndpoint> Discover(IDirectWebRequestHandler requestHandler, XriIdentifier userSuppliedIdentifier) {
			return this.DownloadXrds(requestHandler).CreateServiceEndpoints(userSuppliedIdentifier);
		}

		/// <summary>
		/// Returns an <see cref="Identifier"/> that has no URI fragment.
		/// Quietly returns the original <see cref="Identifier"/> if it is not
		/// a <see cref="UriIdentifier"/> or no fragment exists.
		/// </summary>
		/// <returns>
		/// A new <see cref="Identifier"/> instance if there was a
		/// fragment to remove, otherwise this same instance..
		/// </returns>
		/// <remarks>
		/// XRI Identifiers never have a fragment part, and thus this method
		/// always returns this same instance.
		/// </remarks>
		internal override Identifier TrimFragment() {
			return this;
		}

		/// <summary>
		/// Converts a given identifier to its secure equivalent.
		/// UriIdentifiers originally created with an implied HTTP scheme change to HTTPS.
		/// Discovery is made to require SSL for the entire resolution process.
		/// </summary>
		/// <param name="secureIdentifier">The newly created secure identifier.
		/// If the conversion fails, <paramref name="secureIdentifier"/> retains
		/// <i>this</i> identifiers identity, but will never discover any endpoints.</param>
		/// <returns>
		/// True if the secure conversion was successful.
		/// False if the Identifier was originally created with an explicit HTTP scheme.
		/// </returns>
		internal override bool TryRequireSsl(out Identifier secureIdentifier) {
			secureIdentifier = IsDiscoverySecureEndToEnd ? this : new XriIdentifier(this, true);
			return true;
		}

		/// <summary>
		/// Takes any valid form of XRI string and returns the canonical form of the same XRI.
		/// </summary>
		/// <param name="xri">The xri to canonicalize.</param>
		/// <returns>The canonicalized form of the XRI.</returns>
		/// <remarks>The canonical form, per the OpenID spec, is no scheme and no whitespace on either end.</remarks>
		private static string CanonicalizeXri(string xri) {
			xri = xri.Trim();
			if (xri.StartsWith(XriScheme, StringComparison.OrdinalIgnoreCase)) {
				xri = xri.Substring(XriScheme.Length);
			}
			return xri;
		}

		/// <summary>
		/// Downloads the XRDS document for this XRI.
		/// </summary>
		/// <param name="requestHandler">The request handler.</param>
		/// <returns>The XRDS document.</returns>
		private XrdsDocument DownloadXrds(IDirectWebRequestHandler requestHandler) {
			XrdsDocument doc;
			using (var xrdsResponse = Yadis.Request(requestHandler, this.XrdsUrl, this.IsDiscoverySecureEndToEnd)) {
				doc = new XrdsDocument(XmlReader.Create(xrdsResponse.ResponseStream));
			}
			ErrorUtilities.VerifyProtocol(doc.IsXrdResolutionSuccessful, OpenIdStrings.XriResolutionFailed);
			return doc;
		}
	}
}
