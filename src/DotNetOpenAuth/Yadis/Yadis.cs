//-----------------------------------------------------------------------
// <copyright file="Yadis.cs" company="Andrew Arnott, Scott Hanselman">
//     Copyright (c) Andrew Arnott, Scott Hanselman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Yadis {
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Cache;
	using System.Web.UI.HtmlControls;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.Xrds;

	internal class Yadis {
		internal const string HeaderName = "X-XRDS-Location";

		/// <summary>
		/// Gets or sets the cache that can be used for HTTP requests made during identifier discovery.
		/// </summary>
		internal static readonly RequestCachePolicy IdentifierDiscoveryCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);

		/// <summary>
		/// Performs YADIS discovery on some identifier.
		/// </summary>
		/// <param name="requestHandler">The mechanism to use for sending HTTP requests.</param>
		/// <param name="uri">The URI to perform discovery on.</param>
		/// <param name="requireSsl">Whether discovery should fail if any step of it is not encrypted.</param>
		/// <returns>
		/// The result of discovery on the given URL.
		/// Null may be returned if an error occurs,
		/// or if <paramref name="requireSsl"/> is true but part of discovery
		/// is not protected by SSL.
		/// </returns>
		public static DiscoveryResult Discover(IDirectSslWebRequestHandler requestHandler, UriIdentifier uri, bool requireSsl) {
			DirectWebResponse response;
			try {
				if (requireSsl && !string.Equals(uri.Uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
					Logger.WarnFormat("Discovery on insecure identifier '{0}' aborted.", uri);
					return null;
				}
				response = Request(requestHandler, uri, requireSsl, ContentTypes.Html, ContentTypes.XHtml, ContentTypes.Xrds);
				response.CacheNetworkStreamAndClose();
				if (response.Status != System.Net.HttpStatusCode.OK) {
					return null;
				}
			} catch (ArgumentException ex) {
				// Unsafe URLs generate this
				Logger.WarnFormat("Unsafe OpenId URL detected ({0}).  Request aborted.  {1}", uri, ex);
				return null;
			}
			DirectWebResponse response2 = null;
			if (IsXrdsDocument(response)) {
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
				if (url == null && response.ContentType.MediaType == ContentTypes.Html) {
					url = FindYadisDocumentLocationInHtmlMetaTags(response.Body);
					if (url != null) {
						Logger.DebugFormat("{0} found in HTML Http-Equiv tag.  Preparing to pull XRDS from {1}", HeaderName, url);
					}
				}
				if (url != null) {
					if (!requireSsl || string.Equals(url.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
						response2 = Request(requestHandler, url, requireSsl);
						response2.CacheNetworkStreamAndClose();
						if (response2.Status != System.Net.HttpStatusCode.OK) {
							return null;
						}
					} else {
						Logger.WarnFormat("XRDS document at insecure location '{0}'.  Aborting YADIS discovery.", url);
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
						if (Uri.TryCreate(metaTag.Content, UriKind.Absolute, out uri)) {
							return uri;
						}
					}
				}
			}
			return null;
		}

		internal static DirectWebResponse Request(IDirectSslWebRequestHandler requestHandler, Uri uri, bool requireSsl, params string[] acceptTypes) {
			ErrorUtilities.VerifyArgumentNotNull(uri, "uri");

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.CachePolicy = IdentifierDiscoveryCachePolicy;
			if (acceptTypes != null) {
				request.Accept = string.Join(",", acceptTypes);
			}

			return requestHandler.GetResponse(request, requireSsl);
		}

		private static bool IsXrdsDocument(DirectWebResponse response) {
			if (response.ContentType.MediaType == ContentTypes.Xrds) {
				return true;
			}

			if (response.ContentType.MediaType == ContentTypes.Xml) {
				// This COULD be an XRDS document with an imprecise content-type.
				XmlReader reader = XmlReader.Create(new StringReader(response.Body));
				while (reader.Read() && reader.NodeType != XmlNodeType.Element) {
					// intentionally blank
				}
				if (reader.NamespaceURI == XrdsNode.XrdsNamespace && reader.Name == "XRDS") {
					return true;
				}
			}

			return false;
		}
	}
}
