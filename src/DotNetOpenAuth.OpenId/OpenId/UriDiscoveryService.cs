//-----------------------------------------------------------------------
// <copyright file="UriDiscoveryService.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web.UI.HtmlControls;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Xrds;
	using DotNetOpenAuth.Yadis;

	/// <summary>
	/// The discovery service for URI identifiers.
	/// </summary>
	public class UriDiscoveryService : IIdentifierDiscoveryService {
		/// <summary>
		/// Initializes a new instance of the <see cref="UriDiscoveryService"/> class.
		/// </summary>
		public UriDiscoveryService() {
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
			var uriIdentifier = identifier as UriIdentifier;
			if (uriIdentifier == null) {
				return Enumerable.Empty<IdentifierDiscoveryResult>();
			}

			var endpoints = new List<IdentifierDiscoveryResult>();

			// Attempt YADIS discovery
			DiscoveryResult yadisResult = Yadis.Discover(requestHandler, uriIdentifier, identifier.IsDiscoverySecureEndToEnd);
			if (yadisResult != null) {
				if (yadisResult.IsXrds) {
					try {
						XrdsDocument xrds = new XrdsDocument(yadisResult.ResponseText);
						var xrdsEndpoints = xrds.XrdElements.CreateServiceEndpoints(yadisResult.NormalizedUri, uriIdentifier);

						// Filter out insecure endpoints if high security is required.
						if (uriIdentifier.IsDiscoverySecureEndToEnd) {
							xrdsEndpoints = xrdsEndpoints.Where(se => se.ProviderEndpoint.IsTransportSecure());
						}
						endpoints.AddRange(xrdsEndpoints);
					} catch (XmlException ex) {
						Logger.Yadis.Error("Error while parsing the XRDS document.  Falling back to HTML discovery.", ex);
					}
				}

				// Failing YADIS discovery of an XRDS document, we try HTML discovery.
				if (endpoints.Count == 0) {
					yadisResult.TryRevertToHtmlResponse();
					var htmlEndpoints = new List<IdentifierDiscoveryResult>(DiscoverFromHtml(yadisResult.NormalizedUri, uriIdentifier, yadisResult.ResponseText));
					if (htmlEndpoints.Any()) {
						Logger.Yadis.DebugFormat("Total services discovered in HTML: {0}", htmlEndpoints.Count);
						Logger.Yadis.Debug(htmlEndpoints.ToStringDeferred(true));
						endpoints.AddRange(htmlEndpoints.Where(ep => !uriIdentifier.IsDiscoverySecureEndToEnd || ep.ProviderEndpoint.IsTransportSecure()));
						if (endpoints.Count == 0) {
							Logger.Yadis.Info("No HTML discovered endpoints met the security requirements.");
						}
					} else {
						Logger.Yadis.Debug("HTML discovery failed to find any endpoints.");
					}
				} else {
					Logger.Yadis.Debug("Skipping HTML discovery because XRDS contained service endpoints.");
				}
			}
			return endpoints;
		}

		#endregion

		/// <summary>
		/// Searches HTML for the HEAD META tags that describe OpenID provider services.
		/// </summary>
		/// <param name="claimedIdentifier">The final URL that provided this HTML document.
		/// This may not be the same as (this) userSuppliedIdentifier if the
		/// userSuppliedIdentifier pointed to a 301 Redirect.</param>
		/// <param name="userSuppliedIdentifier">The user supplied identifier.</param>
		/// <param name="html">The HTML that was downloaded and should be searched.</param>
		/// <returns>
		/// A sequence of any discovered ServiceEndpoints.
		/// </returns>
		private static IEnumerable<IdentifierDiscoveryResult> DiscoverFromHtml(Uri claimedIdentifier, UriIdentifier userSuppliedIdentifier, string html) {
			var linkTags = new List<HtmlLink>(HtmlParser.HeadTags<HtmlLink>(html));
			foreach (var protocol in Protocol.AllPracticalVersions) {
				// rel attributes are supposed to be interpreted with case INsensitivity, 
				// and is a space-delimited list of values. (http://www.htmlhelp.com/reference/html40/values.html#linktypes)
				var serverLinkTag = linkTags.WithAttribute("rel").FirstOrDefault(tag => Regex.IsMatch(tag.Attributes["rel"], @"\b" + Regex.Escape(protocol.HtmlDiscoveryProviderKey) + @"\b", RegexOptions.IgnoreCase));
				if (serverLinkTag == null) {
					continue;
				}

				Uri providerEndpoint = null;
				if (Uri.TryCreate(serverLinkTag.Href, UriKind.Absolute, out providerEndpoint)) {
					// See if a LocalId tag of the discovered version exists
					Identifier providerLocalIdentifier = null;
					var delegateLinkTag = linkTags.WithAttribute("rel").FirstOrDefault(tag => Regex.IsMatch(tag.Attributes["rel"], @"\b" + Regex.Escape(protocol.HtmlDiscoveryLocalIdKey) + @"\b", RegexOptions.IgnoreCase));
					if (delegateLinkTag != null) {
						if (Identifier.IsValid(delegateLinkTag.Href)) {
							providerLocalIdentifier = delegateLinkTag.Href;
						} else {
							Logger.Yadis.WarnFormat("Skipping endpoint data because local id is badly formed ({0}).", delegateLinkTag.Href);
							continue; // skip to next version
						}
					}

					// Choose the TypeURI to match the OpenID version detected.
					string[] typeURIs = { protocol.ClaimedIdentifierServiceTypeURI };
					yield return IdentifierDiscoveryResult.CreateForClaimedIdentifier(
						claimedIdentifier,
						userSuppliedIdentifier,
						providerLocalIdentifier,
						new ProviderEndpointDescription(providerEndpoint, typeURIs),
						(int?)null,
						(int?)null);
				}
			}
		}
	}
}
