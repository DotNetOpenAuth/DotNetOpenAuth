using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Mime;
using System.Web.UI.HtmlControls;
using System.Diagnostics;

namespace DotNetOpenId.Yadis {
	class Yadis {
		internal const string HeaderName = "X-XRDS-Location";

		/// <summary>
		/// Performs YADIS discovery on some identifier.
		/// </summary>
		/// <param name="uri">The URI to perform discovery on.</param>
		/// <param name="requireSsl">Whether discovery should fail if any step of it is not encrypted.</param>
		/// <returns>
		/// The result of discovery on the given URL.
		/// Null may be returned if an error occurs,
		/// or if <paramref name="requireSsl"/> is true but part of discovery
		/// is not protected by SSL.
		/// </returns>
		public static DiscoveryResult Discover(UriIdentifier uri, bool requireSsl) {
			UntrustedWebResponse response;
			try {
				if (requireSsl && !string.Equals(uri.Uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
					Logger.WarnFormat("Discovery on insecure identifier '{0}' aborted.", uri);
					return null;
				}
				response = UntrustedWebRequest.Request(uri, null,
					new[] { ContentTypes.Html, ContentTypes.XHtml, ContentTypes.Xrds }, requireSsl);
				if (response.StatusCode != System.Net.HttpStatusCode.OK) {
					Logger.ErrorFormat("HTTP error {0} {1} while performing discovery on {2}.", (int)response.StatusCode, response.StatusCode, uri);
					return null;
				}
			} catch (ArgumentException ex) {
				// Unsafe URLs generate this
				Logger.WarnFormat("Unsafe OpenId URL detected ({0}).  Request aborted.  {1}", uri, ex);
				return null;
			}
			UntrustedWebResponse response2 = null;
			if (isXrdsDocument(response)) {
				Logger.Debug("An XRDS response was received from GET at user-supplied identifier.");
				response2 = response;
			} else {
				string uriString = response.Headers.Get(HeaderName);
				Uri url = null;
				if (uriString != null) {
					if (Uri.TryCreate(uriString, UriKind.Absolute, out url)) {
						Logger.DebugFormat("{0} found in HTTP header.  Preparing to pull XRDS from {1}", HeaderName, url);
					}
				}
				if (url == null && (response.ContentType.MediaType == ContentTypes.Html || response.ContentType.MediaType == ContentTypes.XHtml)) {
					url = FindYadisDocumentLocationInHtmlMetaTags(response.ReadResponseString());
					if (url != null) {
						Logger.DebugFormat("{0} found in HTML Http-Equiv tag.  Preparing to pull XRDS from {1}", HeaderName, url);
					}
				}
				if (url != null) {
					if (!requireSsl || string.Equals(url.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
						response2 = UntrustedWebRequest.Request(url, null, new[] { ContentTypes.Xrds }, requireSsl);
						if (response2.StatusCode != System.Net.HttpStatusCode.OK) {
							return null;
						}
					} else {
						Logger.WarnFormat("XRDS document at insecure location '{0}'.  Aborting YADIS discovery.", url);
					}
				}
			}
			return new DiscoveryResult(uri, response, response2);
		}

		private static bool isXrdsDocument(UntrustedWebResponse response) {
			if (response.ContentType.MediaType == ContentTypes.Xrds) {
				return true;
			}

			if (response.ContentType.MediaType == ContentTypes.Xml) {
				// This COULD be an XRDS document with an imprecise content-type.
				XmlReader reader = XmlReader.Create(new StringReader(response.ReadResponseString()));
				while (reader.Read() && reader.NodeType != XmlNodeType.Element) ;
				if (reader.NamespaceURI == XrdsNode.XrdsNamespace && reader.Name == "XRDS") {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Searches an HTML document for a 
		/// &lt;meta http-equiv="X-XRDS-Location" content="{YadisURL}"&gt;
		/// tag and returns the content of YadisURL.
		/// </summary>
		public static Uri FindYadisDocumentLocationInHtmlMetaTags(string html) {
			foreach (var metaTag in HtmlParser.HeadTags<HtmlMeta>(html)) {
				if (HeaderName.Equals(metaTag.HttpEquiv, StringComparison.OrdinalIgnoreCase)) {
					if (metaTag.Content != null) {
						Uri uri;
						if (Uri.TryCreate(metaTag.Content, UriKind.Absolute, out uri))
							return uri;
					}
				}
			}
			return null;
		}
	}

	class DiscoveryResult {
		public DiscoveryResult(Uri requestUri, UntrustedWebResponse initialResponse, UntrustedWebResponse finalResponse) {
			RequestUri = requestUri;
			NormalizedUri = initialResponse.FinalUri;
			if (finalResponse == null) {
				ContentType = initialResponse.ContentType;
				ResponseText = initialResponse.ReadResponseString();
				IsXrds = ContentType.MediaType == ContentTypes.Xrds;
			} else {
				ContentType = finalResponse.ContentType;
				ResponseText = finalResponse.ReadResponseString();
				IsXrds = true;
				if (initialResponse != finalResponse) {
					YadisLocation = finalResponse.RequestUri;
				}
			}
		}

		/// <summary>
		/// The URI of the original YADIS discovery request.  
		/// This is the user supplied Identifier as given in the original
		/// YADIS discovery request.
		/// </summary>
		public Uri RequestUri { get; private set; }
		/// <summary>
		/// The fully resolved (after redirects) URL of the user supplied Identifier.
		/// This becomes the ClaimedIdentifier.
		/// </summary>
		public Uri NormalizedUri { get; private set; }
		/// <summary>
		/// The location the XRDS document was downloaded from, if different
		/// from the user supplied Identifier.
		/// </summary>
		public Uri YadisLocation { get; private set; }
		/// <summary>
		/// The Content-Type associated with the <see cref="ResponseText"/>.
		/// </summary>
		public ContentType ContentType { get; private set; }
		/// <summary>
		/// The text in the final response.
		/// This may be an XRDS document or it may be an HTML document, 
		/// as determined by the <see cref="IsXrds"/> property.
		/// </summary>
		public string ResponseText { get; private set; }
		/// <summary>
		/// Whether the <see cref="ResponseText"/> represents an XRDS document.
		/// False if the response is an HTML document.
		/// </summary>
		public bool IsXrds { get; private set; }
		/// <summary>
		/// True if the response to the userSuppliedIdentifier pointed to a different URL
		/// for the XRDS document.
		/// </summary>
		public bool UsedYadisLocation {
			get { return YadisLocation != null; }
		}
	}
}
