using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Mime;
using System.Web.UI.HtmlControls;

namespace DotNetOpenId.Yadis {
	class Yadis {
		internal const string HeaderName = "X-XRDS-Location";

		public static DiscoveryResult Discover(UriIdentifier uri) {
			var response = RelyingParty.Fetcher.Request(uri, null,
				new[] { ContentTypes.Html, ContentTypes.XHtml, ContentTypes.Xrds });
			if (response.StatusCode != System.Net.HttpStatusCode.OK) {
				return null;
			}
			RelyingParty.FetchResponse response2 = null;
			if (response.ContentType.MediaType == ContentTypes.Xrds) {
				response2 = response;
			} else {
				string uriString = response.Headers.Get(HeaderName.ToLower());
				Uri url = null;
				if (uriString != null)
					Uri.TryCreate(uriString, UriKind.Absolute, out url);
				if (url == null && response.ContentType.MediaType == ContentTypes.Html)
					url = FindYadisDocumentLocationInHtmlMetaTags(response.ReadResponseString());
				if (url != null) {
					response2 = RelyingParty.Fetcher.Request(url);
					if (response2.StatusCode != System.Net.HttpStatusCode.OK) {
						return null;
					}
				}
			}
			return new DiscoveryResult(uri, response, response2);
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
		public DiscoveryResult(Uri requestUri, RelyingParty.FetchResponse initialResponse, RelyingParty.FetchResponse finalResponse) {
			RequestUri = requestUri;
			NormalizedUri = initialResponse.FinalUri;
			if (finalResponse == null) {
				ContentType = initialResponse.ContentType;
				ResponseText = initialResponse.ReadResponseString();
			} else {
				ContentType = finalResponse.ContentType;
				ResponseText = finalResponse.ReadResponseString();
			}
			if ((initialResponse != finalResponse) && (finalResponse != null)) {
				YadisLocation = finalResponse.RequestUri;
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
		public bool IsXrds {
			get { return UsedYadisLocation || ContentType.MediaType == ContentTypes.Xrds; }
		}
		/// <summary>
		/// True if the response to the userSuppliedIdentifier pointed to a different URL
		/// for the XRDS document.
		/// </summary>
		public bool UsedYadisLocation {
			get { return YadisLocation != null; }
		}
	}
}
