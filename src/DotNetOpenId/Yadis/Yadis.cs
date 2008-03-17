using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Xml;
using Janrain.Yadis;
using System.Xml.Serialization;

namespace DotNetOpenId.Yadis {
	internal class Yadis {
		internal static class ContentTypes {
			public const string Html = "text/html";
			public const string XHtml = "application/xhtml+xml";
			public const string Xrds = "application/xrds+xml";
		}
		const string headerName = "X-XRDS-Location";

		public static DiscoveryResult Discover(Identifier userSuppliedIdentifier) {
			if (userSuppliedIdentifier == null) throw new ArgumentNullException("userSuppliedIdentifier");
			XriIdentifier xriIdentifier = userSuppliedIdentifier as XriIdentifier;
			UriIdentifier uriIdentifier = userSuppliedIdentifier as UriIdentifier;
			if (xriIdentifier != null)
				return discoverXri(xriIdentifier);
			if (uriIdentifier != null)
				return discoverUri(uriIdentifier);
			throw new ArgumentException(null, "userSuppliedIdentifier");
		}

		static DiscoveryResult discoverUri(Uri uri) {
			FetchRequest request = new FetchRequest(uri);
			FetchResponse response = request.GetResponse(true);
			if (response.StatusCode != System.Net.HttpStatusCode.OK) {
				return null;
			}
			FetchResponse response2 = null;
			if (response.ContentType.MediaType == ContentTypes.Xrds) {
				response2 = response;
			} else {
				string uriString = response.Headers.Get(headerName.ToLower());
				Uri url = null;
				if (uriString != null)
					Uri.TryCreate(uriString, UriKind.Absolute, out url);
				if (url == null && response.ContentType.MediaType == ContentTypes.Html)
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

		static DiscoveryResult discoverXri(XriIdentifier xri) {
			var res = new XriResolver(xri.CanonicalXri);
			var xriResolverResponse = DotNetOpenId.RelyingParty.Fetcher.Request(res.Resolver);
			MemoryStream ms = new MemoryStream(xriResolverResponse.Data, 0, xriResolverResponse.Length);
			var reader = XmlReader.Create(ms);
			var xrds = new XrdsDocument(reader);
			foreach (var xrd in xrds.XrdElements) {
				foreach (var service in xrd.OpenIdServices) {
					foreach (var uri in service.UriElements) {
						DiscoveryResult result = discoverUri(uri.Uri);
						if (result != null) return result;
					}
				}
			}
			return null;
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
				if (headerName.Equals(text, StringComparison.OrdinalIgnoreCase)) {
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

	[Serializable]
	internal class DiscoveryResult {
		public DiscoveryResult(Uri requestUri, FetchResponse initResp, FetchResponse finalResp) {
			// requestUri == null when XRI is used.
			RequestUri = requestUri == null ? initResp.FinalUri : requestUri;
			NormalizedUri = initResp.FinalUri;
			if (finalResp == null) {
				ContentType = initResp.ContentType;
				ResponseText = initResp.Body;
			} else {
				ContentType = finalResp.ContentType;
				ResponseText = finalResp.Body;
			}
			if ((initResp != finalResp) && (finalResp != null)) {
				YadisLocation = finalResp.RequestUri;
			}
		}

		public ContentType ContentType { get; private set; }
		public Uri NormalizedUri { get; private set; }
		public Uri RequestUri { get; private set; }
		public string ResponseText { get; private set; }
		public Uri YadisLocation { get; private set; }

		public bool IsXRDS {
			get {
				return UsedYadisLocation || ContentType.MediaType == Yadis.ContentTypes.Xrds;
			}
		}

		public bool UsedYadisLocation {
			get { return YadisLocation != null; }
		}
	}
}
