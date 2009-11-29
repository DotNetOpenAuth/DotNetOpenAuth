//-----------------------------------------------------------------------
// <copyright file="HostMetaDiscoveryService.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.XPath;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Xrds;
	using DotNetOpenAuth.Yadis;

	/// <summary>
	/// The discovery service to support Google Apps for Domains' proprietary discovery.
	/// </summary>
	/// <remarks>
	/// The spec for this discovery mechanism can be found at:
	/// http://groups.google.com/group/google-federated-login-api/web/openid-discovery-for-hosted-domains
	/// and the XMLDSig spec referenced in that spec can be found at:
	/// http://wiki.oasis-open.org/xri/XrdOne/XmlDsigProfile
	/// </remarks>
	public class HostMetaDiscoveryService : IIdentifierDiscoveryService {
		/// <summary>
		/// The URI template for discovery host-meta on domains hosted by
		/// Google Apps for Domains.
		/// </summary>
		private const string GoogleHostedHostMeta = "https://www.google.com/accounts/o8/.well-known/host-meta?hd={0}";

		private const string LocalHostMetaPath = "/.well-known/host-meta";

		private static readonly Regex HostMetaLink = new Regex(@"^Link: <(?<location>.+?)>; rel=""describedby http://reltype.google.com/openid/xrd-op""; type=""application/xrds\+xml""$");

		/// <summary>
		/// Initializes a new instance of the <see cref="HostMetaDiscoveryService"/> class.
		/// </summary>
		public HostMetaDiscoveryService() {
			this.TrustedHostMetaProxies = new List<string>();
		}

		/// <summary>
		/// Gets the set of URI templates to use to contact host-meta hosting proxies
		/// for domain discovery.
		/// </summary>
		public IList<string> TrustedHostMetaProxies { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether to trust Google to host domains' host-meta documents.
		/// </summary>
		/// <remarks>
		/// This property is just a convenient mechanism for checking or changing the set of
		/// trusted host-meta proxies in the <see cref="TrustedHostMetaProxies"/> property.
		/// </remarks>
		public bool UseGoogleHostedHostMeta {
			get {
				return this.TrustedHostMetaProxies.Contains(GoogleHostedHostMeta);
			}

			set {
				if (value != this.UseGoogleHostedHostMeta) {
					if (value) {
						this.TrustedHostMetaProxies.Add(GoogleHostedHostMeta);
					} else {
						this.TrustedHostMetaProxies.Remove(GoogleHostedHostMeta);
					}
				}
			}
		}

		#region IIdentifierDiscoveryService Members

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

			// Google Apps are always URIs -- not XRIs.
			var uriIdentifier = identifier as UriIdentifier;
			if (uriIdentifier == null) {
				return Enumerable.Empty<IdentifierDiscoveryResult>();
			}

			var results = new List<IdentifierDiscoveryResult>();
			var response = GetXrdsResponse(uriIdentifier, requestHandler);

			if (response != null) {
				try {
					var document = new XrdsDocument(XmlReader.Create(response.ResponseStream));
					ValidateXmlDSig(document, uriIdentifier, response);
					var xrds = GetXrdElements(document, uriIdentifier.Uri.Host);

					// Find OP identifiers
					results.AddRange(xrds.CreateServiceEndpoints(uriIdentifier, uriIdentifier));

					// Look for claimed identifier template URIs for an additional XRDS document.
					results.AddRange(GetExternalServices(xrds, uriIdentifier, requestHandler));

					abortDiscoveryChain = true;
				} catch (XmlException ex) {
					Logger.Yadis.ErrorFormat("Error while parsing XRDS document at {0} pointed to by host-meta: {1}", response.FinalUri, ex);
				}
			}

			return results;
		}

		#endregion

		private static IEnumerable<XrdElement> GetXrdElements(XrdsDocument document, string canonicalId) {
			// filter to include only those XRD elements describing the host whose host-meta pointed us to this document.
			return document.XrdElements.Where(xrd => string.Equals(xrd.CanonicalID, canonicalId, StringComparison.Ordinal));
		}

		private static IEnumerable<ServiceElement> GetDescribedByServices(IEnumerable<XrdElement> xrds) {
			var describedBy = from xrd in xrds
							  from service in xrd.SearchForServiceTypeUris(p => "http://www.iana.org/assignments/relation/describedby")
							  select service;
			return describedBy;
		}

		private static IEnumerable<IdentifierDiscoveryResult> GetExternalServices(IEnumerable<XrdElement> xrds, UriIdentifier identifier, IDirectWebRequestHandler requestHandler) {
			Contract.Requires<ArgumentNullException>(xrds != null);
			Contract.Requires<ArgumentNullException>(identifier != null);
			Contract.Requires<ArgumentNullException>(requestHandler != null);
			Contract.Ensures(Contract.Result<IEnumerable<IdentifierDiscoveryResult>>() != null);

			var results = new List<IdentifierDiscoveryResult>();
			foreach (var serviceElement in GetDescribedByServices(xrds)) {
				var template = serviceElement.Node.SelectSingleNode("google:URITemplate", serviceElement.XmlNamespaceResolver);
				var nextAuthority = serviceElement.Node.SelectSingleNode("google:NextAuthority", serviceElement.XmlNamespaceResolver);
				if (template != null && nextAuthority != null) {
					Uri externalLocation = new Uri(template.Value.Trim().Replace("{%uri}", Uri.EscapeDataString(identifier.Uri.AbsoluteUri)));
					try {
						var externalXrdsResponse = GetXrdsResponse(identifier, requestHandler, externalLocation);
						XrdsDocument externalXrds = new XrdsDocument(XmlReader.Create(externalXrdsResponse.ResponseStream));
						ValidateXmlDSig(externalXrds, identifier, externalXrdsResponse/*, signer: nextAuthority.Value.Trim()*/);
						results.AddRange(GetXrdElements(externalXrds, identifier).CreateServiceEndpoints(identifier, identifier));
					} catch (ProtocolException ex) {
						Logger.Yadis.WarnFormat("HTTP GET error while retrieving described-by XRDS document {0}: {1}", externalLocation.AbsoluteUri, ex);
					} catch (XmlException ex) {
						Logger.Yadis.ErrorFormat("Error while parsing described-by XRDS document {0}: {1}", externalLocation.AbsoluteUri, ex);
					}
				}
			}

			return results;
		}

		private static void ValidateXmlDSig(XrdsDocument document, UriIdentifier identifier, IncomingWebResponse response) {
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(identifier != null);
			Contract.Requires<ArgumentNullException>(response != null);

			var signatureNode = document.Node.SelectSingleNode("/xrds:XRDS/ds:Signature", document.XmlNamespaceResolver);
			ErrorUtilities.VerifyProtocol(signatureNode != null, "Missing Signature element.");
			var signedInfoNode = signatureNode.SelectSingleNode("ds:SignedInfo", document.XmlNamespaceResolver);
			ErrorUtilities.VerifyProtocol(signedInfoNode != null, "Missing SignedInfo element.");
			ErrorUtilities.VerifyProtocol(
				signedInfoNode.SelectSingleNode("ds:CanonicalizationMethod[@Algorithm='http://docs.oasis-open.org/xri/xrd/2009/01#canonicalize-raw-octets']", document.XmlNamespaceResolver) != null,
				"Unrecognized or missing canonicalization method.");
			ErrorUtilities.VerifyProtocol(
				signedInfoNode.SelectSingleNode("ds:SignatureMethod[@Algorithm='http://www.w3.org/2000/09/xmldsig#rsa-sha1']", document.XmlNamespaceResolver) != null,
				"Unrecognized or missing signature method.");
			var certNodes = signatureNode.Select("ds:KeyInfo/ds:X509Data/ds:X509Certificate", document.XmlNamespaceResolver);
			ErrorUtilities.VerifyProtocol(certNodes.Count > 0, "Missing X509Certificate element.");
			var certs = certNodes.Cast<XPathNavigator>().Select(n => new X509Certificate2(Convert.FromBase64String(n.Value.Trim()))).ToList();

			// Verify that we trust the signer of the certificates.
			////ErrorUtilities.VerifyProtocol(certs.All(cert => cert.Verify()), "Invalid or untrusted signing certificate.");

			// Verify that the certificate is issued to the host on whom we are performing discovery.
			string hostName = certs[0].GetNameInfo(X509NameType.DnsName, false);
			////ErrorUtilities.VerifyProtocol(string.Equals(hostName, identifier.Uri.Host, StringComparison.OrdinalIgnoreCase), "X.509 signing certificate issued to {0} but used to sign XRD describing {1}", hostName, identifier.Uri.Host);
			// TODO: code here

			// Verify the signature itself
			byte[] signature = Convert.FromBase64String(response.Headers["Signature"]);
			var provider = (RSACryptoServiceProvider)certs.First().PublicKey.Key;
			byte[] data = new byte[response.ResponseStream.Length];
			response.ResponseStream.Seek(0, SeekOrigin.Begin);
			response.ResponseStream.Read(data, 0, data.Length);
			ErrorUtilities.VerifyProtocol(provider.VerifyData(data, "SHA1", signature), "Invalid XmlDSig signature on XRDS document.");
		}

		private static IncomingWebResponse GetXrdsResponse(UriIdentifier identifier, IDirectWebRequestHandler requestHandler, Uri xrdsLocation) {
			Contract.Requires<ArgumentNullException>(identifier != null);
			Contract.Requires<ArgumentNullException>(requestHandler != null);
			Contract.Requires<ArgumentNullException>(xrdsLocation != null);
			Contract.Ensures(Contract.Result<IncomingWebResponse>() != null);

			var request = (HttpWebRequest)WebRequest.Create(xrdsLocation);
			request.CachePolicy = Yadis.IdentifierDiscoveryCachePolicy;
			request.Accept = ContentTypes.Xrds;
			var options = identifier.IsDiscoverySecureEndToEnd ? DirectWebRequestOptions.RequireSsl : DirectWebRequestOptions.None;
			var response = requestHandler.GetResponse(request, options);
			if (!string.Equals(response.ContentType.MediaType, ContentTypes.Xrds, StringComparison.Ordinal)) {
				Logger.Yadis.WarnFormat("Host-meta pointed to XRDS at {0}, but Content-Type at that URL was unexpected value '{1}'.", xrdsLocation, response.ContentType);
			}

			return response;
		}

		private IncomingWebResponse GetXrdsResponse(UriIdentifier identifier, IDirectWebRequestHandler requestHandler) {
			Contract.Requires<ArgumentNullException>(identifier != null);
			Contract.Requires<ArgumentNullException>(requestHandler != null);
			Uri xrdsLocation = this.GetXrdsLocation(identifier, requestHandler);
			if (xrdsLocation == null) {
				return null;
			}

			var response = GetXrdsResponse(identifier, requestHandler, xrdsLocation);

			return response;
		}

		private Uri GetXrdsLocation(UriIdentifier identifier, IDirectWebRequestHandler requestHandler) {
			Contract.Requires<ArgumentNullException>(identifier != null);
			Contract.Requires<ArgumentNullException>(requestHandler != null);
			var hostMetaResponse = this.GetHostMeta(identifier, requestHandler);
			if (hostMetaResponse == null) {
				return null;
			}

			using (var sr = hostMetaResponse.GetResponseReader()) {
				string line = sr.ReadLine();
				Match m = HostMetaLink.Match(line);
				if (m.Success) {
					Uri location = new Uri(m.Groups["location"].Value);
					Logger.Yadis.InfoFormat("Found link to XRDS at {0} in host-meta document {1}.", location, hostMetaResponse.FinalUri);
					return location;
				}
			}

			Logger.Yadis.WarnFormat("Could not find link to XRDS in host-meta document: {0}", hostMetaResponse.FinalUri);
			return null;
		}

		/// <summary>
		/// Gets the host-meta for a given identifier.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="requestHandler">The request handler.</param>
		/// <returns>The host-meta response, or <c>null</c> if no host-meta document could be obtained.</returns>
		private IncomingWebResponse GetHostMeta(UriIdentifier identifier, IDirectWebRequestHandler requestHandler) {
			Contract.Requires<ArgumentNullException>(identifier != null);
			Contract.Requires<ArgumentNullException>(requestHandler != null);
			foreach (Uri hostMetaLocation in this.GetHostMetaLocations(identifier)) {
				var request = (HttpWebRequest)WebRequest.Create(hostMetaLocation);
				request.CachePolicy = Yadis.IdentifierDiscoveryCachePolicy;
				var options = DirectWebRequestOptions.AcceptAllHttpResponses;
				if (identifier.IsDiscoverySecureEndToEnd) {
					options |= DirectWebRequestOptions.RequireSsl;
				}
				var response = requestHandler.GetResponse(request, options);
				if (response.Status == HttpStatusCode.OK) {
					Logger.Yadis.InfoFormat("Found host-meta for {0} at: {1}", identifier.Uri.Host, hostMetaLocation);
					return response;
				} else {
					Logger.Yadis.InfoFormat("Could not obtain host-meta for {0} from {1}", identifier.Uri.Host, hostMetaLocation);
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the URIs authorized to host host-meta documents on behalf of a given domain.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <returns>A sequence of URIs that MAY provide the host-meta for a given identifier.</returns>
		private IEnumerable<Uri> GetHostMetaLocations(UriIdentifier identifier) {
			Contract.Requires<ArgumentNullException>(identifier != null);
			// First try the proxies, as they are considered more "secure" than the local
			// host-meta for a domain since the domain may be defaced.
			foreach (string proxyPattern in this.TrustedHostMetaProxies) {
				yield return new Uri(string.Format(CultureInfo.InvariantCulture, proxyPattern, identifier.Uri.Host));
			}

			// Finally, look for the local host-meta.
			UriBuilder localHostMetaBuilder = new UriBuilder();
			localHostMetaBuilder.Scheme = identifier.IsDiscoverySecureEndToEnd || identifier.Uri.IsTransportSecure() ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
			localHostMetaBuilder.Host = identifier.Uri.Host;
			localHostMetaBuilder.Path = LocalHostMetaPath;
			yield return localHostMetaBuilder.Uri;
		}
	}
}
