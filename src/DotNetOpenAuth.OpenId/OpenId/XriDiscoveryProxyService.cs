//-----------------------------------------------------------------------
// <copyright file="XriDiscoveryProxyService.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Xml;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Xrds;
	using DotNetOpenAuth.Yadis;

	/// <summary>
	/// The discovery service for XRI identifiers that uses an XRI proxy resolver for discovery.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xri", Justification = "Acronym")]
	public class XriDiscoveryProxyService : IIdentifierDiscoveryService {
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
		private const string XriResolverProxyTemplate = "https://{1}/{0}?_xrd_r=application/xrd%2Bxml;sep=false";

		/// <summary>
		/// Initializes a new instance of the <see cref="XriDiscoveryProxyService"/> class.
		/// </summary>
		public XriDiscoveryProxyService() {
		}

		#region IDiscoveryService Members

		/// <summary>
		/// Performs discovery on the specified identifier.
		/// </summary>
		/// <param name="identifier">The identifier to perform discovery on.</param>
		/// <param name="requestHandler">The means to place outgoing HTTP requests.</param>
		/// <param name="abortDiscoveryChain">if set to <c>true</c>, no further discovery services will be called for this identifier.</param>
		/// <returns>
		/// A sequence of service endpoints yielded by discovery.  Must not be null, but may be empty.
		/// </returns>
		public IEnumerable<IdentifierDiscoveryResult> Discover(Identifier identifier, IDirectWebRequestHandler requestHandler, out bool abortDiscoveryChain) {
			abortDiscoveryChain = false;
			var xriIdentifier = identifier as XriIdentifier;
			if (xriIdentifier == null) {
				return Enumerable.Empty<IdentifierDiscoveryResult>();
			}

			return DownloadXrds(xriIdentifier, requestHandler).XrdElements.CreateServiceEndpoints(xriIdentifier);
		}

		#endregion

		/// <summary>
		/// Downloads the XRDS document for this XRI.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="requestHandler">The request handler.</param>
		/// <returns>The XRDS document.</returns>
		private static XrdsDocument DownloadXrds(XriIdentifier identifier, IDirectWebRequestHandler requestHandler) {
			Requires.NotNull(identifier, "identifier");
			Requires.NotNull(requestHandler, "requestHandler");
			Contract.Ensures(Contract.Result<XrdsDocument>() != null);
			XrdsDocument doc;
			using (var xrdsResponse = Yadis.Request(requestHandler, GetXrdsUrl(identifier), identifier.IsDiscoverySecureEndToEnd)) {
				var readerSettings = MessagingUtilities.CreateUntrustedXmlReaderSettings();
				doc = new XrdsDocument(XmlReader.Create(xrdsResponse.ResponseStream, readerSettings));
			}
			ErrorUtilities.VerifyProtocol(doc.IsXrdResolutionSuccessful, OpenIdStrings.XriResolutionFailed);
			return doc;
		}

		/// <summary>
		/// Gets the URL from which this XRI's XRDS document may be downloaded.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <returns>The URI to HTTP GET from to get the services.</returns>
		private static Uri GetXrdsUrl(XriIdentifier identifier) {
			ErrorUtilities.VerifyProtocol(OpenIdElement.Configuration.XriResolver.Enabled, OpenIdStrings.XriResolutionDisabled);
			string xriResolverProxy = XriResolverProxyTemplate;
			if (identifier.IsDiscoverySecureEndToEnd) {
				// Indicate to xri.net that we require SSL to be used for delegated resolution
				// of community i-names.
				xriResolverProxy += ";https=true";
			}

			return new Uri(
				string.Format(
					CultureInfo.InvariantCulture,
					xriResolverProxy,
					identifier,
					OpenIdElement.Configuration.XriResolver.Proxy.Name));
		}
	}
}
