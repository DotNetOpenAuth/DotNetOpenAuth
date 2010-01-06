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
	using System.Security;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Security.Permissions;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.XPath;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Xrds;
	using DotNetOpenAuth.Yadis;

	/// <summary>
	/// The discovery service to support host-meta based discovery, such as Google Apps for Domains.
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
		private static readonly HostMetaProxy GoogleHostedHostMeta = new HostMetaProxy("https://www.google.com/accounts/o8/.well-known/host-meta?hd={0}", "hosted-id.google.com");

		/// <summary>
		/// Path to the well-known location of the host-meta document at a domain.
		/// </summary>
		private const string LocalHostMetaPath = "/.well-known/host-meta";

		/// <summary>
		/// The pattern within a host-meta file to look for to obtain the URI to the XRDS document.
		/// </summary>
		private static readonly Regex HostMetaLink = new Regex(@"^Link: <(?<location>.+?)>; rel=""describedby http://reltype.google.com/openid/xrd-op""; type=""application/xrds\+xml""$");

		/// <summary>
		/// Initializes a new instance of the <see cref="HostMetaDiscoveryService"/> class.
		/// </summary>
		public HostMetaDiscoveryService() {
			this.TrustedHostMetaProxies = new List<HostMetaProxy>();
		}

		/// <summary>
		/// Gets the set of URI templates to use to contact host-meta hosting proxies
		/// for domain discovery.
		/// </summary>
		public IList<HostMetaProxy> TrustedHostMetaProxies { get; private set; }

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
			string signingHost;
			var response = GetXrdsResponse(uriIdentifier, requestHandler, out signingHost);

			if (response != null) {
				try {
					var document = new XrdsDocument(XmlReader.Create(response.ResponseStream));
					ValidateXmlDSig(document, uriIdentifier, response, signingHost);
					var xrds = GetXrdElements(document, uriIdentifier.Uri.Host);

					// Look for claimed identifier template URIs for an additional XRDS document.
					results.AddRange(GetExternalServices(xrds, uriIdentifier, requestHandler));

					// If we couldn't find any claimed identifiers, look for OP identifiers.
					// Normally this would be the opposite (OP Identifiers take precedence over
					// claimed identifiers, but for Google Apps, XRDS' always have OP Identifiers
					// mixed in, which the OpenID spec mandate should eclipse Claimed Identifiers,
					// which would break positive assertion checks).
					if (results.Count == 0) {
						results.AddRange(xrds.CreateServiceEndpoints(uriIdentifier, uriIdentifier));
					}

					abortDiscoveryChain = true;
				} catch (XmlException ex) {
					Logger.Yadis.ErrorFormat("Error while parsing XRDS document at {0} pointed to by host-meta: {1}", response.FinalUri, ex);
				}
			}

			return results;
		}

		#endregion

		/// <summary>
		/// Gets the XRD elements that have a given CanonicalID.
		/// </summary>
		/// <param name="document">The XRDS document.</param>
		/// <param name="canonicalId">The CanonicalID to match on.</param>
		/// <returns>A sequence of XRD elements.</returns>
		private static IEnumerable<XrdElement> GetXrdElements(XrdsDocument document, string canonicalId) {
			// filter to include only those XRD elements describing the host whose host-meta pointed us to this document.
			return document.XrdElements.Where(xrd => string.Equals(xrd.CanonicalID, canonicalId, StringComparison.Ordinal));
		}

		/// <summary>
		/// Gets the described-by services in XRD elements.
		/// </summary>
		/// <param name="xrds">The XRDs to search.</param>
		/// <returns>A sequence of services.</returns>
		private static IEnumerable<ServiceElement> GetDescribedByServices(IEnumerable<XrdElement> xrds) {
			Contract.Requires<ArgumentNullException>(xrds != null);
			Contract.Ensures(Contract.Result<IEnumerable<ServiceElement>>() != null);

			var describedBy = from xrd in xrds
							  from service in xrd.SearchForServiceTypeUris(p => "http://www.iana.org/assignments/relation/describedby")
							  select service;
			return describedBy;
		}

		/// <summary>
		/// Gets the services for an identifier that are described by an external XRDS document.
		/// </summary>
		/// <param name="xrds">The XRD elements to search for described-by services.</param>
		/// <param name="identifier">The identifier under discovery.</param>
		/// <param name="requestHandler">The request handler.</param>
		/// <returns>The discovered services.</returns>
		private static IEnumerable<IdentifierDiscoveryResult> GetExternalServices(IEnumerable<XrdElement> xrds, UriIdentifier identifier, IDirectWebRequestHandler requestHandler) {
			Contract.Requires<ArgumentNullException>(xrds != null);
			Contract.Requires<ArgumentNullException>(identifier != null);
			Contract.Requires<ArgumentNullException>(requestHandler != null);
			Contract.Ensures(Contract.Result<IEnumerable<IdentifierDiscoveryResult>>() != null);

			var results = new List<IdentifierDiscoveryResult>();
			foreach (var serviceElement in GetDescribedByServices(xrds)) {
				var templateNode = serviceElement.Node.SelectSingleNode("google:URITemplate", serviceElement.XmlNamespaceResolver);
				var nextAuthorityNode = serviceElement.Node.SelectSingleNode("google:NextAuthority", serviceElement.XmlNamespaceResolver);
				if (templateNode != null) {
					Uri externalLocation = new Uri(templateNode.Value.Trim().Replace("{%uri}", Uri.EscapeDataString(identifier.Uri.AbsoluteUri)));
					string nextAuthority = nextAuthorityNode != null ? nextAuthorityNode.Value.Trim() : identifier.Uri.Host;
					try {
						var externalXrdsResponse = GetXrdsResponse(identifier, requestHandler, externalLocation);
						XrdsDocument externalXrds = new XrdsDocument(XmlReader.Create(externalXrdsResponse.ResponseStream));
						ValidateXmlDSig(externalXrds, identifier, externalXrdsResponse, nextAuthority);
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

		/// <summary>
		/// Validates the XML digital signature on an XRDS document.
		/// </summary>
		/// <param name="document">The XRDS document whose signature should be validated.</param>
		/// <param name="identifier">The identifier under discovery.</param>
		/// <param name="response">The response.</param>
		/// <param name="signingHost">The host name on the certificate that should be used to verify the signature in the XRDS.</param>
		/// <exception cref="ProtocolException">Thrown if the XRDS document has an invalid or a missing signature.</exception>
		private static void ValidateXmlDSig(XrdsDocument document, UriIdentifier identifier, IncomingWebResponse response, string signingHost) {
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
			// Start by trying to validate just the certificate used to sign the XRDS document,
			// since we can do that with partial trust.
			if (!certs[0].Verify()) {
				// We couldn't verify just the signing certificate, so try to verify the whole certificate chain.
				try {
					VerifyCertChain(certs);
				} catch (SecurityException) {
					Logger.Yadis.Warn("Signing certificate verification failed and we have insufficient code access security permissions to perform certificate chain validation.");
					ErrorUtilities.ThrowProtocol(OpenIdStrings.X509CertificateNotTrusted);
				}
			}

			// Verify that the certificate is issued to the host on whom we are performing discovery.
			string hostName = certs[0].GetNameInfo(X509NameType.DnsName, false);
			ErrorUtilities.VerifyProtocol(string.Equals(hostName, signingHost, StringComparison.OrdinalIgnoreCase), "X.509 signing certificate issued to {0}, but a certificate for {1} was expected.", hostName, signingHost);

			// Verify the signature itself
			byte[] signature = Convert.FromBase64String(response.Headers["Signature"]);
			var provider = (RSACryptoServiceProvider)certs.First().PublicKey.Key;
			byte[] data = new byte[response.ResponseStream.Length];
			response.ResponseStream.Seek(0, SeekOrigin.Begin);
			response.ResponseStream.Read(data, 0, data.Length);
			ErrorUtilities.VerifyProtocol(provider.VerifyData(data, "SHA1", signature), "Invalid XmlDSig signature on XRDS document.");
		}

		/// <summary>
		/// Verifies the cert chain.
		/// </summary>
		/// <param name="certs">The certs.</param>
		/// <remarks>
		/// This must be in a method of its own because there is a LinkDemand on the <see cref="X509Chain.Build"/>
		/// method.  By being in a method of its own, the caller of this method may catch a
		/// <see cref="SecurityException"/> that is thrown if we're not running with full trust and execute
		/// an alternative plan.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the certificate chain is invalid or unverifiable.</exception>
		private static void VerifyCertChain(List<X509Certificate2> certs) {
			var chain = new X509Chain();
			foreach (var cert in certs) {
				chain.Build(cert);
			}

			if (chain.ChainStatus.Length > 0) {
				ErrorUtilities.ThrowProtocol(
					string.Format(
						CultureInfo.CurrentCulture,
						OpenIdStrings.X509CertificateNotTrusted + " {0}",
						string.Join(", ", chain.ChainStatus.Select(status => status.StatusInformation).ToArray())));
			}
		}

		/// <summary>
		/// Gets the XRDS HTTP response for a given identifier.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="requestHandler">The request handler.</param>
		/// <param name="xrdsLocation">The location of the XRDS document to retrieve.</param>
		/// <returns>
		/// A HTTP response carrying an XRDS document.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if the XRDS document could not be obtained.</exception>
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

		/// <summary>
		/// Gets the XRDS HTTP response for a given identifier.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="requestHandler">The request handler.</param>
		/// <param name="signingHost">The host name on the certificate that should be used to verify the signature in the XRDS.</param>
		/// <returns>A HTTP response carrying an XRDS document, or <c>null</c> if one could not be obtained.</returns>
		/// <exception cref="ProtocolException">Thrown if the XRDS document could not be obtained.</exception>
		private IncomingWebResponse GetXrdsResponse(UriIdentifier identifier, IDirectWebRequestHandler requestHandler, out string signingHost) {
			Contract.Requires<ArgumentNullException>(identifier != null);
			Contract.Requires<ArgumentNullException>(requestHandler != null);
			Uri xrdsLocation = this.GetXrdsLocation(identifier, requestHandler, out signingHost);
			if (xrdsLocation == null) {
				return null;
			}

			var response = GetXrdsResponse(identifier, requestHandler, xrdsLocation);

			return response;
		}

		/// <summary>
		/// Gets the location of the XRDS document that describes a given identifier.
		/// </summary>
		/// <param name="identifier">The identifier under discovery.</param>
		/// <param name="requestHandler">The request handler.</param>
		/// <param name="signingHost">The host name on the certificate that should be used to verify the signature in the XRDS.</param>
		/// <returns>An absolute URI, or <c>null</c> if one could not be determined.</returns>
		private Uri GetXrdsLocation(UriIdentifier identifier, IDirectWebRequestHandler requestHandler, out string signingHost) {
			Contract.Requires<ArgumentNullException>(identifier != null);
			Contract.Requires<ArgumentNullException>(requestHandler != null);
			var hostMetaResponse = this.GetHostMeta(identifier, requestHandler, out signingHost);
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
		/// <param name="signingHost">The host name on the certificate that should be used to verify the signature in the XRDS.</param>
		/// <returns>
		/// The host-meta response, or <c>null</c> if no host-meta document could be obtained.
		/// </returns>
		private IncomingWebResponse GetHostMeta(UriIdentifier identifier, IDirectWebRequestHandler requestHandler, out string signingHost) {
			Contract.Requires<ArgumentNullException>(identifier != null);
			Contract.Requires<ArgumentNullException>(requestHandler != null);
			foreach (var hostMetaProxy in this.GetHostMetaLocations(identifier)) {
				var hostMetaLocation = hostMetaProxy.GetProxy(identifier);
				var request = (HttpWebRequest)WebRequest.Create(hostMetaLocation);
				request.CachePolicy = Yadis.IdentifierDiscoveryCachePolicy;
				var options = DirectWebRequestOptions.AcceptAllHttpResponses;
				if (identifier.IsDiscoverySecureEndToEnd) {
					options |= DirectWebRequestOptions.RequireSsl;
				}
				var response = requestHandler.GetResponse(request, options);
				if (response.Status == HttpStatusCode.OK) {
					Logger.Yadis.InfoFormat("Found host-meta for {0} at: {1}", identifier.Uri.Host, hostMetaLocation);
					signingHost = hostMetaProxy.GetSigningHost(identifier);
					return response;
				} else {
					Logger.Yadis.InfoFormat("Could not obtain host-meta for {0} from {1}", identifier.Uri.Host, hostMetaLocation);
				}
			}

			signingHost = null;
			return null;
		}

		/// <summary>
		/// Gets the URIs authorized to host host-meta documents on behalf of a given domain.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <returns>A sequence of URIs that MAY provide the host-meta for a given identifier.</returns>
		private IEnumerable<HostMetaProxy> GetHostMetaLocations(UriIdentifier identifier) {
			Contract.Requires<ArgumentNullException>(identifier != null);

			// First try the proxies, as they are considered more "secure" than the local
			// host-meta for a domain since the domain may be defaced.
			IEnumerable<HostMetaProxy> result = this.TrustedHostMetaProxies;

			// Finally, look for the local host-meta.
			UriBuilder localHostMetaBuilder = new UriBuilder();
			localHostMetaBuilder.Scheme = identifier.IsDiscoverySecureEndToEnd || identifier.Uri.IsTransportSecure() ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
			localHostMetaBuilder.Host = identifier.Uri.Host;
			localHostMetaBuilder.Path = LocalHostMetaPath;
			result = result.Concat(new[] { new HostMetaProxy(localHostMetaBuilder.Uri.AbsoluteUri, identifier.Uri.Host) });

			return result;
		}

		/// <summary>
		/// A description of a web server that hosts host-meta documents.
		/// </summary>
		public class HostMetaProxy {
			/// <summary>
			/// Initializes a new instance of the <see cref="HostMetaProxy"/> class.
			/// </summary>
			/// <param name="proxyFormat">The proxy formatting string.</param>
			/// <param name="signingHostFormat">The signing host formatting string.</param>
			public HostMetaProxy(string proxyFormat, string signingHostFormat) {
				Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(proxyFormat));
				Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(signingHostFormat));
				this.ProxyFormat = proxyFormat;
				this.SigningHostFormat = signingHostFormat;
			}

			/// <summary>
			/// Gets the URL of the host-meta proxy.
			/// </summary>
			/// <value>The absolute proxy URL, which may include {0} to be replaced with the host of the identifier to be discovered.</value>
			public string ProxyFormat { get; private set; }

			/// <summary>
			/// Gets the formatting string to determine the expected host name on the certificate
			/// that is expected to be used to sign the XRDS document.
			/// </summary>
			/// <value>
			/// Either a string literal, or a formatting string where these placeholders may exist:
			/// {0} the host on the identifier discovery was originally performed on;
			/// {1} the host on this proxy.
			/// </value>
			public string SigningHostFormat { get; private set; }

			/// <summary>
			/// Gets the absolute proxy URI.
			/// </summary>
			/// <param name="identifier">The identifier being discovered.</param>
			/// <returns>The an absolute URI.</returns>
			public virtual Uri GetProxy(UriIdentifier identifier) {
				Contract.Requires<ArgumentNullException>(identifier != null);
				return new Uri(string.Format(CultureInfo.InvariantCulture, this.ProxyFormat, Uri.EscapeDataString(identifier.Uri.Host)));
			}

			/// <summary>
			/// Gets the signing host URI.
			/// </summary>
			/// <param name="identifier">The identifier being discovered.</param>
			/// <returns>A host name.</returns>
			public virtual string GetSigningHost(UriIdentifier identifier) {
				Contract.Requires<ArgumentNullException>(identifier != null);
				return string.Format(CultureInfo.InvariantCulture, this.SigningHostFormat, identifier.Uri.Host, this.GetProxy(identifier).Host);
			}

			/// <summary>
			/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
			/// </summary>
			/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
			/// <returns>
			/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
			/// </returns>
			/// <exception cref="T:System.NullReferenceException">
			/// The <paramref name="obj"/> parameter is null.
			/// </exception>
			public override bool Equals(object obj) {
				var other = obj as HostMetaProxy;
				if (other == null) {
					return false;
				}

				return this.ProxyFormat == other.ProxyFormat && this.SigningHostFormat == other.SigningHostFormat;
			}

			/// <summary>
			/// Serves as a hash function for a particular type.
			/// </summary>
			/// <returns>
			/// A hash code for the current <see cref="T:System.Object"/>.
			/// </returns>
			public override int GetHashCode() {
				return this.ProxyFormat.GetHashCode();
			}
		}
	}
}
