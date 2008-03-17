using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Xml;
using Janrain.Yadis;
using System.Xml.Serialization;

namespace DotNetOpenId.Yadis {
	class Yadis {
		internal const string HeaderName = "X-XRDS-Location";

		public static DiscoveryResult Discover(UriIdentifier uri) {
			FetchRequest request = new FetchRequest(uri);
			FetchResponse response = request.GetResponse(true);
			if (response.StatusCode != System.Net.HttpStatusCode.OK) {
				return null;
			}
			FetchResponse response2 = null;
			if (response.ContentType.MediaType == ContentType.Xrds) {
				response2 = response;
			} else {
				string uriString = response.Headers.Get(HeaderName.ToLower());
				Uri url = null;
				if (uriString != null)
					Uri.TryCreate(uriString, UriKind.Absolute, out url);
				if (url == null && response.ContentType.MediaType == ContentType.Html)
					url = FindYadisDocumentLocationInHtmlMetaTags(response.Body);
				if (url != null) {
					response2 = new FetchRequest(url).GetResponse(false);
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
			NameValueCollection[] nvc = ByteParser.HeadTagAttrs(html, "meta");
			foreach (var values in nvc) {
				string text = values["http-equiv"];
				if (HeaderName.Equals(text, StringComparison.OrdinalIgnoreCase)) {
					string uriString = values.Get("content");
					if (uriString != null) {
						Uri uri;
						if (Uri.TryCreate(uriString, UriKind.Absolute, out uri))
							return uri;
					}
				}
			}
			return null;
		}
	}

	class DiscoveryResult {
		public DiscoveryResult(Uri requestUri, FetchResponse initialResponse, FetchResponse finalResponse) {
			RequestUri = requestUri;
			NormalizedUri = initialResponse.FinalUri;
			if (finalResponse == null) {
				ContentType = initialResponse.ContentType;
				ResponseText = initialResponse.Body;
			} else {
				ContentType = finalResponse.ContentType;
				ResponseText = finalResponse.Body;
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
			get { return UsedYadisLocation || ContentType.MediaType == ContentType.Xrds; }
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
