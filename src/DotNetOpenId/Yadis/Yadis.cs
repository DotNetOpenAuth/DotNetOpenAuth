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

		public static DiscoveryResult Discover(UriIdentifier uri) {
			UntrustedWebResponse response;
			try {
				response = UntrustedWebRequest.Request(uri, null,
				new[] { ContentTypes.Html, ContentTypes.XHtml, ContentTypes.Xrds });
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
				response2 = response;
			} else {
				string uriString = response.Headers.Get(HeaderName);
				Uri url = null;
				if (uriString != null)
					Uri.TryCreate(uriString, UriKind.Absolute, out url);
				if (url == null && response.ContentType.MediaType == ContentTypes.Html)
					url = FindYadisDocumentLocationInHtmlMetaTags(response.ReadResponseString());
				if (url != null) {
					response2 = UntrustedWebRequest.Request(url);
					if (response2.StatusCode != System.Net.HttpStatusCode.OK) {
						return null;
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
