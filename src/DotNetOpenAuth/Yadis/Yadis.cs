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

	/// <summary>
	/// YADIS discovery manager.
	/// </summary>
	internal class Yadis {
		/// <summary>
		/// The HTTP header to look for in responses to declare where the XRDS document should be found.
		/// </summary>
		internal const string HeaderName = "X-XRDS-Location";

		/// <summary>
		/// Gets or sets the cache that can be used for HTTP requests made during identifier discovery.
		/// </summary>
		internal static readonly RequestCachePolicy IdentifierDiscoveryCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);

		/// <summary>
		/// The maximum number of bytes to read from an HTTP response
		/// in searching for a link to a YADIS document.
		/// </summary>
		private const int MaximumResultToScan = 1024 * 1024;

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
		public static DiscoveryResult Discover(IDirectWebRequestHandler requestHandler, UriIdentifier uri, bool requireSsl) {
			CachedDirectWebResponse response;
			try {
				if (requireSsl && !string.Equals(uri.Uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
					Logger.WarnFormat("Discovery on insecure identifier '{0}' aborted.", uri);
					return null;
				}
				response = Request(requestHandler, uri, requireSsl, ContentTypes.Html, ContentTypes.XHtml, ContentTypes.Xrds).GetSnapshot(MaximumResultToScan);
				if (response.Status != System.Net.HttpStatusCode.OK) {
					Logger.ErrorFormat("HTTP error {0} {1} while performing discovery on {2}.", (int)response.Status, response.Status, uri);
					return null;
				}
			} catch (ArgumentException ex) {
				// Unsafe URLs generate this
				Logger.WarnFormat("Unsafe OpenId URL detected ({0}).  Request aborted.  {1}", uri, ex);
				return null;
			}
			CachedDirectWebResponse response2 = null;
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
						response2 = Request(requestHandler, url, requireSsl, ContentTypes.Xrds).GetSnapshot(MaximumResultToScan);
						if (response2.Status != HttpStatusCode.OK) {
							Logger.ErrorFormat("HTTP error {0} {1} while performing discovery on {2}.", (int)response2.Status, response2.Status, uri);
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
		/// <param name="html">The HTML to search.</param>
		/// <returns>The URI of the XRDS document if found; otherwise <c>null</c>.</returns>
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

		/// <summary>
		/// Sends a YADIS HTTP request as part of identifier discovery.
		/// </summary>
		/// <param name="requestHandler">The request handler to use to actually submit the request.</param>
		/// <param name="uri">The URI to GET.</param>
		/// <param name="requireSsl">Whether only HTTPS URLs should ever be retrieved.</param>
		/// <param name="acceptTypes">The value of the Accept HTTP header to include in the request.</param>
		/// <returns>The HTTP response retrieved from the request.</returns>
		internal static DirectWebResponse Request(IDirectWebRequestHandler requestHandler, Uri uri, bool requireSsl, params string[] acceptTypes) {
			ErrorUtilities.VerifyArgumentNotNull(requestHandler, "requestHandler");
			ErrorUtilities.VerifyArgumentNotNull(uri, "uri");

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.CachePolicy = IdentifierDiscoveryCachePolicy;
			if (acceptTypes != null) {
				request.Accept = string.Join(",", acceptTypes);
			}

			DirectWebRequestOptions options = DirectWebRequestOptions.None;
			if (requireSsl) {
				options |= DirectWebRequestOptions.RequireSsl;
			}

			return requestHandler.GetResponse(request, options);
		}

		/// <summary>
		/// Determines whether a given HTTP response constitutes an XRDS document.
		/// </summary>
		/// <param name="response">The response to test.</param>
		/// <returns>
		/// 	<c>true</c> if the response constains an XRDS document; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsXrdsDocument(CachedDirectWebResponse response) {
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
