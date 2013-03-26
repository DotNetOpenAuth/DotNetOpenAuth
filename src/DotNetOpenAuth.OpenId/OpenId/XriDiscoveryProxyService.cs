//-----------------------------------------------------------------------
// <copyright file="XriDiscoveryProxyService.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Net.Http;
	using System.Runtime.CompilerServices;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Xrds;
	using DotNetOpenAuth.Yadis;
	using Validation;

	/// <summary>
	/// The discovery service for XRI identifiers that uses an XRI proxy resolver for discovery.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xri", Justification = "Acronym")]
	public class XriDiscoveryProxyService : IIdentifierDiscoveryService, IRequireHostFactories {
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

		/// <summary>
		/// Gets or sets the host factories used by this instance.
		/// </summary>
		public IHostFactories HostFactories { get; set; }

		#region IDiscoveryService Members

		/// <summary>
		/// Performs discovery on the specified identifier.
		/// </summary>
		/// <param name="identifier">The identifier to perform discovery on.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of service endpoints yielded by discovery.  Must not be null, but may be empty.
		/// </returns>
		public async Task<IdentifierDiscoveryServiceResult> DiscoverAsync(Identifier identifier, CancellationToken cancellationToken) {
			Requires.NotNull(identifier, "identifier");
			Verify.Operation(this.HostFactories != null, Strings.HostFactoriesRequired);

			var xriIdentifier = identifier as XriIdentifier;
			if (xriIdentifier == null) {
				return new IdentifierDiscoveryServiceResult(Enumerable.Empty<IdentifierDiscoveryResult>());
			}

			var xrds = await DownloadXrdsAsync(xriIdentifier, this.HostFactories, cancellationToken);
			var endpoints = xrds.XrdElements.CreateServiceEndpoints(xriIdentifier);
			return new IdentifierDiscoveryServiceResult(endpoints);
		}

		#endregion

		/// <summary>
		/// Downloads the XRDS document for this XRI.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="hostFactories">The host factories.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The XRDS document.
		/// </returns>
		private static async Task<XrdsDocument> DownloadXrdsAsync(XriIdentifier identifier, IHostFactories hostFactories, CancellationToken cancellationToken) {
			Requires.NotNull(identifier, "identifier");
			Requires.NotNull(hostFactories, "hostFactories");

			XrdsDocument doc;
			using (var xrdsResponse = await Yadis.RequestAsync(GetXrdsUrl(identifier), identifier.IsDiscoverySecureEndToEnd, hostFactories, cancellationToken)) {
				xrdsResponse.EnsureSuccessStatusCode();
				var readerSettings = MessagingUtilities.CreateUntrustedXmlReaderSettings();
				ErrorUtilities.VerifyProtocol(xrdsResponse.Content != null, "XRDS request \"{0}\" returned no response.", GetXrdsUrl(identifier));
				await xrdsResponse.Content.LoadIntoBufferAsync();
				using (var xrdsStream = await xrdsResponse.Content.ReadAsStreamAsync()) {
					doc = new XrdsDocument(XmlReader.Create(xrdsStream, readerSettings));
				}
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
