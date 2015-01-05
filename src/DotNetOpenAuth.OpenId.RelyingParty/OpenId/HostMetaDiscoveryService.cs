//-----------------------------------------------------------------------
// <copyright file="HostMetaDiscoveryService.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Cache;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Security;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Security.Permissions;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml;
	using System.Xml.XPath;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Xrds;
	using DotNetOpenAuth.Yadis;
	using Validation;

	/// <summary>
	/// The discovery service to support host-meta based discovery, such as Google Apps for Domains.
	/// </summary>
	/// <remarks>
	/// The spec for this discovery mechanism can be found at:
	/// http://groups.google.com/group/google-federated-login-api/web/openid-discovery-for-hosted-domains
	/// and the XMLDSig spec referenced in that spec can be found at:
	/// http://wiki.oasis-open.org/xri/XrdOne/XmlDsigProfile
	/// </remarks>
	public class HostMetaDiscoveryService : IIdentifierDiscoveryService, IRequireHostFactories {
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
		/// A set of certificate thumbprints that have been verified.
		/// </summary>
		private static readonly HashSet<string> ApprovedCertificateThumbprintCache = new HashSet<string>(StringComparer.Ordinal);

		/// <summary>
		/// Initializes a new instance of the <see cref="HostMetaDiscoveryService"/> class.
		/// </summary>
		public HostMetaDiscoveryService() {
			this.TrustedHostMetaProxies = new List<HostMetaProxy>();
		}

		/// <summary>
		/// Gets or sets the host factories used by this instance.
		/// </summary>
		/// <value>
		/// The host factories.
		/// </value>
		public IHostFactories HostFactories { get; set; }

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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of service endpoints yielded by discovery.  Must not be null, but may be empty.
		/// </returns>
		public async Task<IdentifierDiscoveryServiceResult> DiscoverAsync(Identifier identifier, CancellationToken cancellationToken) {
			Requires.NotNull(identifier, "identifier");
			Verify.Operation(this.HostFactories != null, Strings.HostFactoriesRequired);
			cancellationToken.ThrowIfCancellationRequested();

			// Google Apps are always URIs -- not XRIs.
			var uriIdentifier = identifier as UriIdentifier;
			if (uriIdentifier == null) {
				return new IdentifierDiscoveryServiceResult(Enumerable.Empty<IdentifierDiscoveryResult>());
			}

			var results = new List<IdentifierDiscoveryResult>();
			using (var response = await this.GetXrdsResponseAsync(uriIdentifier, cancellationToken)) {
				if (response.Result != null) {
					try {
						var readerSettings = MessagingUtilities.CreateUntrustedXmlReaderSettings();
						var responseStream = await response.Result.Content.ReadAsStreamAsync();
						var document = new XrdsDocument(XmlReader.Create(responseStream, readerSettings));
						await ValidateXmlDSigAsync(document, uriIdentifier, response.Result, response.SigningHost);
						var xrds = GetXrdElements(document, uriIdentifier.Uri.Host);

						// Look for claimed identifier template URIs for an additional XRDS document.
						results.AddRange(await this.GetExternalServicesAsync(xrds, uriIdentifier, cancellationToken));

						// If we couldn't find any claimed identifiers, look for OP identifiers.
						// Normally this would be the opposite (OP Identifiers take precedence over
						// claimed identifiers, but for Google Apps, XRDS' always have OP Identifiers
						// mixed in, which the OpenID spec mandate should eclipse Claimed Identifiers,
						// which would break positive assertion checks).
						if (results.Count == 0) {
							results.AddRange(xrds.CreateServiceEndpoints(uriIdentifier, uriIdentifier));
						}
					} catch (XmlException ex) {
						Logger.Yadis.ErrorFormat("Error while parsing XRDS document at {0} pointed to by host-meta: {1}", response.Result.RequestMessage.RequestUri, ex);
					}
				}
			}

			return new IdentifierDiscoveryServiceResult(results, abortDiscoveryChain: true);
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
			Requires.NotNull(xrds, "xrds");

			var describedBy = from xrd in xrds
							  from service in xrd.SearchForServiceTypeUris(p => "http://www.iana.org/assignments/relation/describedby")
							  select service;
			return describedBy;
		}

		/// <summary>
		/// Validates the XML digital signature on an XRDS document.
		/// </summary>
		/// <param name="document">The XRDS document whose signature should be validated.</param>
		/// <param name="identifier">The identifier under discovery.</param>
		/// <param name="response">The response.</param>
		/// <param name="signingHost">The host name on the certificate that should be used to verify the signature in the XRDS.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if the XRDS document has an invalid or a missing signature.</exception>
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "XmlDSig", Justification = "xml")]
		private static async Task ValidateXmlDSigAsync(XrdsDocument document, UriIdentifier identifier, HttpResponseMessage response, string signingHost) {
			Requires.NotNull(document, "document");
			Requires.NotNull(identifier, "identifier");
			Requires.NotNull(response, "response");

			var signatureNode = document.Node.SelectSingleNode("/xrds:XRDS/ds:Signature", document.XmlNamespaceResolver);
			ErrorUtilities.VerifyProtocol(signatureNode != null, OpenIdStrings.MissingElement, "Signature");
			var signedInfoNode = signatureNode.SelectSingleNode("ds:SignedInfo", document.XmlNamespaceResolver);
			ErrorUtilities.VerifyProtocol(signedInfoNode != null, OpenIdStrings.MissingElement, "SignedInfo");
			ErrorUtilities.VerifyProtocol(
				signedInfoNode.SelectSingleNode("ds:CanonicalizationMethod[@Algorithm='http://docs.oasis-open.org/xri/xrd/2009/01#canonicalize-raw-octets']", document.XmlNamespaceResolver) != null,
				OpenIdStrings.UnsupportedCanonicalizationMethod);
			ErrorUtilities.VerifyProtocol(
				signedInfoNode.SelectSingleNode("ds:SignatureMethod[@Algorithm='http://www.w3.org/2000/09/xmldsig#rsa-sha1']", document.XmlNamespaceResolver) != null,
				OpenIdStrings.UnsupportedSignatureMethod);
			var certNodes = signatureNode.Select("ds:KeyInfo/ds:X509Data/ds:X509Certificate", document.XmlNamespaceResolver);
			ErrorUtilities.VerifyProtocol(certNodes.Count > 0, OpenIdStrings.MissingElement, "X509Certificate");
			var certs = certNodes.Cast<XPathNavigator>().Select(n => new X509Certificate2(Convert.FromBase64String(n.Value.Trim()))).ToList();

			VerifyCertificateChain(certs);

			// Verify that the certificate is issued to the host on whom we are performing discovery.
			string hostName = certs[0].GetNameInfo(X509NameType.DnsName, false);
			ErrorUtilities.VerifyProtocol(string.Equals(hostName, signingHost, StringComparison.OrdinalIgnoreCase), OpenIdStrings.MisdirectedSigningCertificate, hostName, signingHost);

			// Verify the signature itself
			byte[] signature = Convert.FromBase64String(response.Headers.GetValues("Signature").First());
			var provider = (RSACryptoServiceProvider)certs.First().PublicKey.Key;
			var responseStream = await response.Content.ReadAsStreamAsync();
			byte[] data = new byte[responseStream.Length];
			responseStream.Seek(0, SeekOrigin.Begin);
			await responseStream.ReadAsync(data, 0, data.Length);
			ErrorUtilities.VerifyProtocol(provider.VerifyData(data, "SHA1", signature), OpenIdStrings.InvalidDSig);
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
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "DotNetOpenAuth.Messaging.ErrorUtilities.ThrowProtocol(System.String,System.Object[])", Justification = "The localized portion is a string resource already.")]
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "By design")]
		private static void VerifyCertChain(IEnumerable<X509Certificate2> certs) {
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
		/// Verifies that a certificate chain is trusted.
		/// </summary>
		/// <param name="certificates">The chain of certificates to verify.</param>
		private static void VerifyCertificateChain(IList<X509Certificate2> certificates) {
			Requires.NotNullEmptyOrNullElements(certificates, "certificates");

			// Before calling into the OS to validate the certificate, since that can for some bizzare reason hang for 5 seconds
			// on some systems, check a cache of previously verified certificates first.
			if (OpenIdElement.Configuration.RelyingParty.HostMetaDiscovery.EnableCertificateValidationCache) {
				lock (ApprovedCertificateThumbprintCache) {
					// HashSet<T> isn't thread-safe.
					if (ApprovedCertificateThumbprintCache.Contains(certificates[0].Thumbprint)) {
						return;
					}
				}
			}

			// Verify that we trust the signer of the certificates.
			// Start by trying to validate just the certificate used to sign the XRDS document,
			// since we can do that with partial trust.
			Logger.OpenId.Debug("Verifying that we trust the certificate used to sign the discovery document.");
			if (!certificates[0].Verify()) {
				// We couldn't verify just the signing certificate, so try to verify the whole certificate chain.
				try {
					Logger.OpenId.Debug("Verifying the whole certificate chain.");
					VerifyCertChain(certificates);
					Logger.OpenId.Debug("Certificate chain verified.");
				} catch (SecurityException) {
					Logger.Yadis.Warn("Signing certificate verification failed and we have insufficient code access security permissions to perform certificate chain validation.");
					ErrorUtilities.ThrowProtocol(OpenIdStrings.X509CertificateNotTrusted);
				}
			}

			if (OpenIdElement.Configuration.RelyingParty.HostMetaDiscovery.EnableCertificateValidationCache) {
				lock (ApprovedCertificateThumbprintCache) {
					ApprovedCertificateThumbprintCache.Add(certificates[0].Thumbprint);
				}
			}
		}

		/// <summary>
		/// Gets the services for an identifier that are described by an external XRDS document.
		/// </summary>
		/// <param name="xrds">The XRD elements to search for described-by services.</param>
		/// <param name="identifier">The identifier under discovery.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The discovered services.
		/// </returns>
		private async Task<IEnumerable<IdentifierDiscoveryResult>> GetExternalServicesAsync(IEnumerable<XrdElement> xrds, UriIdentifier identifier, CancellationToken cancellationToken) {
			Requires.NotNull(xrds, "xrds");
			Requires.NotNull(identifier, "identifier");

			var results = new List<IdentifierDiscoveryResult>();
			foreach (var serviceElement in GetDescribedByServices(xrds)) {
				var templateNode = serviceElement.Node.SelectSingleNode("google:URITemplate", serviceElement.XmlNamespaceResolver);
				var nextAuthorityNode = serviceElement.Node.SelectSingleNode("google:NextAuthority", serviceElement.XmlNamespaceResolver);
				if (templateNode != null) {
					Uri externalLocation = new Uri(templateNode.Value.Trim().Replace("{%uri}", Uri.EscapeDataString(identifier.Uri.AbsoluteUri)));
					string nextAuthority = nextAuthorityNode != null ? nextAuthorityNode.Value.Trim() : identifier.Uri.Host;
					try {
						using (var externalXrdsResponse = await this.GetXrdsResponseAsync(identifier, externalLocation, cancellationToken)) {
							var readerSettings = MessagingUtilities.CreateUntrustedXmlReaderSettings();
							var responseStream = await externalXrdsResponse.Content.ReadAsStreamAsync();
							XrdsDocument externalXrds = new XrdsDocument(XmlReader.Create(responseStream, readerSettings));
							await ValidateXmlDSigAsync(externalXrds, identifier, externalXrdsResponse, nextAuthority);
							results.AddRange(GetXrdElements(externalXrds, identifier).CreateServiceEndpoints(identifier, identifier));
						}
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
		/// Gets the XRDS HTTP response for a given identifier.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="xrdsLocation">The location of the XRDS document to retrieve.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A HTTP response carrying an XRDS document.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if the XRDS document could not be obtained.</exception>
		private async Task<HttpResponseMessage> GetXrdsResponseAsync(UriIdentifier identifier, Uri xrdsLocation, CancellationToken cancellationToken) {
			Requires.NotNull(identifier, "identifier");
			Requires.NotNull(xrdsLocation, "xrdsLocation");

			using (var httpClient = this.HostFactories.CreateHttpClient(identifier.IsDiscoverySecureEndToEnd, Yadis.IdentifierDiscoveryCachePolicy)) {
				var request = new HttpRequestMessage(HttpMethod.Get, xrdsLocation);
				request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentTypes.Xrds));
				var response = await httpClient.SendAsync(request, cancellationToken);
				try {
					if (!string.Equals(response.Content.Headers.ContentType.MediaType, ContentTypes.Xrds, StringComparison.Ordinal)) {
						Logger.Yadis.WarnFormat(
							"Host-meta pointed to XRDS at {0}, but Content-Type at that URL was unexpected value '{1}'.",
							xrdsLocation,
							response.Content.Headers.ContentType);
					}

					return response;
				} catch {
					response.Dispose();
					throw;
				}
			}
		}

		/// <summary>
		/// Gets the XRDS HTTP response for a given identifier.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A HTTP response carrying an XRDS document, or <c>null</c> if one could not be obtained.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if the XRDS document could not be obtained.</exception>
		private async Task<ResultWithSigningHost<HttpResponseMessage>> GetXrdsResponseAsync(UriIdentifier identifier, CancellationToken cancellationToken) {
			Requires.NotNull(identifier, "identifier");

			var result = await this.GetXrdsLocationAsync(identifier, cancellationToken);
			if (result.Result == null) {
				return new ResultWithSigningHost<HttpResponseMessage>();
			}

			var response = await this.GetXrdsResponseAsync(identifier, result.Result, cancellationToken);
			return new ResultWithSigningHost<HttpResponseMessage>(response, result.SigningHost);
		}

		/// <summary>
		/// Gets the location of the XRDS document that describes a given identifier.
		/// </summary>
		/// <param name="identifier">The identifier under discovery.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// An absolute URI, or <c>null</c> if one could not be determined.
		/// </returns>
		private async Task<ResultWithSigningHost<Uri>> GetXrdsLocationAsync(UriIdentifier identifier, CancellationToken cancellationToken) {
			Requires.NotNull(identifier, "identifier");

			using (var hostMetaResponse = await this.GetHostMetaAsync(identifier, cancellationToken)) {
				if (hostMetaResponse.Result == null) {
					return new ResultWithSigningHost<Uri>();
				}

				using (var sr = new StreamReader(await hostMetaResponse.Result.Content.ReadAsStreamAsync())) {
					string line = await sr.ReadLineAsync();
					Match m = HostMetaLink.Match(line);
					if (m.Success) {
						Uri location = new Uri(m.Groups["location"].Value);
						Logger.Yadis.InfoFormat("Found link to XRDS at {0} in host-meta document {1}.", location, hostMetaResponse.Result.RequestMessage.RequestUri);
						return new ResultWithSigningHost<Uri>(location, hostMetaResponse.SigningHost);
					}
				}

				Logger.Yadis.WarnFormat("Could not find link to XRDS in host-meta document: {0}", hostMetaResponse.Result.RequestMessage.RequestUri);
				return new ResultWithSigningHost<Uri>();
			}
		}

		/// <summary>
		/// Gets the host-meta for a given identifier.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The host-meta response, or <c>null</c> if no host-meta document could be obtained.
		/// </returns>
		private async Task<ResultWithSigningHost<HttpResponseMessage>> GetHostMetaAsync(UriIdentifier identifier, CancellationToken cancellationToken) {
			Requires.NotNull(identifier, "identifier");

			using (var httpClient = this.HostFactories.CreateHttpClient(identifier.IsDiscoverySecureEndToEnd, Yadis.IdentifierDiscoveryCachePolicy)) {
				foreach (var hostMetaProxy in this.GetHostMetaLocations(identifier)) {
					var hostMetaLocation = hostMetaProxy.GetProxy(identifier);
					var response = await httpClient.GetAsync(hostMetaLocation, cancellationToken);
					try {
						if (response.IsSuccessStatusCode) {
							Logger.Yadis.InfoFormat("Found host-meta for {0} at: {1}", identifier.Uri.Host, hostMetaLocation);
							return new ResultWithSigningHost<HttpResponseMessage>(response, hostMetaProxy.GetSigningHost(identifier));
						} else {
							Logger.Yadis.InfoFormat("Could not obtain host-meta for {0} from {1}", identifier.Uri.Host, hostMetaLocation);
							response.Dispose();
						}
					} catch {
						response.Dispose();
						throw;
					}
				}
			}

			return new ResultWithSigningHost<HttpResponseMessage>();
		}

		/// <summary>
		/// Gets the URIs authorized to host host-meta documents on behalf of a given domain.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <returns>A sequence of URIs that MAY provide the host-meta for a given identifier.</returns>
		private IEnumerable<HostMetaProxy> GetHostMetaLocations(UriIdentifier identifier) {
			Requires.NotNull(identifier, "identifier");

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
		/// A compound result of some value with the signing host.
		/// </summary>
		/// <typeparam name="T">The type of the primary result.</typeparam>
		private struct ResultWithSigningHost<T> : IDisposable {
			/// <summary>
			/// Initializes a new instance of the <see cref="ResultWithSigningHost{T}"/> struct.
			/// </summary>
			/// <param name="result">The result.</param>
			/// <param name="signingHost">The signing host.</param>
			internal ResultWithSigningHost(T result, string signingHost)
				: this() {
				this.Result = result;
				this.SigningHost = signingHost;
			}

			/// <summary>
			/// Gets the result.
			/// </summary>
			public T Result { get; private set; }

			/// <summary>
			/// Gets the signing host.
			/// </summary>
			public string SigningHost { get; private set; }

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose() {
				var disposable = this.Result as IDisposable;
				disposable.DisposeIfNotNull();
			}
		}

		/// <summary>
		/// A description of a web server that hosts host-meta documents.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "By design")]
		public class HostMetaProxy {
			/// <summary>
			/// Initializes a new instance of the <see cref="HostMetaProxy"/> class.
			/// </summary>
			/// <param name="proxyFormat">The proxy formatting string.</param>
			/// <param name="signingHostFormat">The signing host formatting string.</param>
			public HostMetaProxy(string proxyFormat, string signingHostFormat) {
				Requires.NotNullOrEmpty(proxyFormat, "proxyFormat");
				Requires.NotNullOrEmpty(signingHostFormat, "signingHostFormat");
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
				Requires.NotNull(identifier, "identifier");
				return new Uri(string.Format(CultureInfo.InvariantCulture, this.ProxyFormat, Uri.EscapeDataString(identifier.Uri.Host)));
			}

			/// <summary>
			/// Gets the signing host URI.
			/// </summary>
			/// <param name="identifier">The identifier being discovered.</param>
			/// <returns>A host name.</returns>
			public virtual string GetSigningHost(UriIdentifier identifier) {
				Requires.NotNull(identifier, "identifier");
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
