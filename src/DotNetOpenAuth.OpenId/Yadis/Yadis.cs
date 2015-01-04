//-----------------------------------------------------------------------
// <copyright file="Yadis.cs" company="Outercurve Foundation, Scott Hanselman">
//     Copyright (c) Outercurve Foundation, Scott Hanselman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Yadis {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Cache;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web.UI.HtmlControls;
	using System.Xml;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.Xrds;
	using Validation;

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
#if DEBUG
		internal static readonly RequestCachePolicy IdentifierDiscoveryCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.BypassCache);
#else
		internal static readonly RequestCachePolicy IdentifierDiscoveryCachePolicy = new HttpRequestCachePolicy(OpenIdElement.Configuration.CacheDiscovery ? HttpRequestCacheLevel.CacheIfAvailable : HttpRequestCacheLevel.BypassCache);
#endif

		/// <summary>
		/// The maximum number of bytes to read from an HTTP response
		/// in searching for a link to a YADIS document.
		/// </summary>
		internal const int MaximumResultToScan = 1024 * 1024;

		/// <summary>
		/// Performs YADIS discovery on some identifier.
		/// </summary>
		/// <param name="hostFactories">The host factories.</param>
		/// <param name="uri">The URI to perform discovery on.</param>
		/// <param name="requireSsl">Whether discovery should fail if any step of it is not encrypted.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The result of discovery on the given URL.
		/// Null may be returned if an error occurs,
		/// or if <paramref name="requireSsl" /> is true but part of discovery
		/// is not protected by SSL.
		/// </returns>
		public static async Task<DiscoveryResult> DiscoverAsync(IHostFactories hostFactories, UriIdentifier uri, bool requireSsl, CancellationToken cancellationToken) {
			Requires.NotNull(hostFactories, "hostFactories");
			Requires.NotNull(uri, "uri");

			HttpResponseMessage response;
			try {
				if (requireSsl && !string.Equals(uri.Uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
					Logger.Yadis.WarnFormat("Discovery on insecure identifier '{0}' aborted.", uri);
					return null;
				}

				response = await RequestAsync(uri, requireSsl, hostFactories, cancellationToken, ContentTypes.Html, ContentTypes.XHtml, ContentTypes.Xrds);
				if (response.StatusCode != System.Net.HttpStatusCode.OK) {
					Logger.Yadis.ErrorFormat("HTTP error {0} {1} while performing discovery on {2}.", (int)response.StatusCode, response.StatusCode, uri);
					return null;
				}

				await response.Content.LoadIntoBufferAsync();
			} catch (ArgumentException ex) {
				// Unsafe URLs generate this
				Logger.Yadis.WarnFormat("Unsafe OpenId URL detected ({0}).  Request aborted.  {1}", uri, ex);
				return null;
			}
			HttpResponseMessage response2 = null;
			if (await IsXrdsDocumentAsync(response)) {
				Logger.Yadis.Debug("An XRDS response was received from GET at user-supplied identifier.");
				Reporting.RecordEventOccurrence("Yadis", "XRDS in initial response");
				response2 = response;
			} else {
				IEnumerable<string> uriStrings;
				string uriString = null;
				if (response.Headers.TryGetValues(HeaderName, out uriStrings)) {
					uriString = uriStrings.FirstOrDefault();
				}

				Uri url = null;
				if (uriString != null) {
					if (Uri.TryCreate(uriString, UriKind.Absolute, out url)) {
						Logger.Yadis.DebugFormat("{0} found in HTTP header.  Preparing to pull XRDS from {1}", HeaderName, url);
						Reporting.RecordEventOccurrence("Yadis", "XRDS referenced in HTTP header");
					}
				}

				var contentType = response.Content.Headers.ContentType;
				if (url == null && contentType != null && (contentType.MediaType == ContentTypes.Html || contentType.MediaType == ContentTypes.XHtml)) {
					url = FindYadisDocumentLocationInHtmlMetaTags(await response.Content.ReadAsStringAsync());
					if (url != null) {
						Logger.Yadis.DebugFormat("{0} found in HTML Http-Equiv tag.  Preparing to pull XRDS from {1}", HeaderName, url);
						Reporting.RecordEventOccurrence("Yadis", "XRDS referenced in HTML");
					}
				}
				if (url != null) {
					if (!requireSsl || string.Equals(url.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
						response2 = await RequestAsync(url, requireSsl, hostFactories, cancellationToken, ContentTypes.Xrds);
						if (response2.StatusCode != HttpStatusCode.OK) {
							Logger.Yadis.ErrorFormat("HTTP error {0} {1} while performing discovery on {2}.", (int)response2.StatusCode, response2.StatusCode, uri);
						}
					} else {
						Logger.Yadis.WarnFormat("XRDS document at insecure location '{0}'.  Aborting YADIS discovery.", url);
					}
				}
			}

			return await DiscoveryResult.CreateAsync(uri, response, response2);
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
		/// <param name="uri">The URI to GET.</param>
		/// <param name="requireSsl">Whether only HTTPS URLs should ever be retrieved.</param>
		/// <param name="hostFactories">The host factories.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="acceptTypes">The value of the Accept HTTP header to include in the request.</param>
		/// <returns>
		/// The HTTP response retrieved from the request.
		/// </returns>
		internal static async Task<HttpResponseMessage> RequestAsync(Uri uri, bool requireSsl, IHostFactories hostFactories, CancellationToken cancellationToken, params string[] acceptTypes) {
			Requires.NotNull(uri, "uri");
			Requires.NotNull(hostFactories, "hostFactories");

			using (var httpClient = hostFactories.CreateHttpClient(requireSsl, IdentifierDiscoveryCachePolicy)) {
				var request = new HttpRequestMessage(HttpMethod.Get, uri);
				if (acceptTypes != null) {
					request.Headers.Accept.AddRange(acceptTypes.Select(at => new MediaTypeWithQualityHeaderValue(at)));
				}

				HttpResponseMessage response = null;
				try {
					// http://stackoverflow.com/questions/14103154/how-to-determine-if-an-httpresponsemessage-was-fulfilled-from-cache-using-httpcl
					response = await httpClient.SendAsync(request, cancellationToken);
					if (!response.IsSuccessStatusCode && response.Headers.Age.HasValue && response.Headers.Age.Value > TimeSpan.Zero) {
						// We don't want to report error responses from the cache, since the server may have fixed
						// whatever was causing the problem.  So try again with cache disabled.
						Logger.Messaging.ErrorFormat("An HTTP {0} response was obtained from the cache.  Retrying with cache disabled.", response.StatusCode);
						response.Dispose(); // discard the old one

						var nonCachingRequest = request.Clone();
						using (var nonCachingHttpClient = hostFactories.CreateHttpClient(requireSsl, new RequestCachePolicy(RequestCacheLevel.Reload))) {
							response = await nonCachingHttpClient.SendAsync(nonCachingRequest, cancellationToken);
						}
					}

					return response;
				} catch {
					response.DisposeIfNotNull();
					throw;
				}
			}
		}

		/// <summary>
		/// Determines whether a given HTTP response constitutes an XRDS document.
		/// </summary>
		/// <param name="response">The response to test.</param>
		/// <returns>
		/// 	<c>true</c> if the response constains an XRDS document; otherwise, <c>false</c>.
		/// </returns>
		private static async Task<bool> IsXrdsDocumentAsync(HttpResponseMessage response) {
			if (response.Content.Headers.ContentType == null) {
				return false;
			}

			if (response.Content.Headers.ContentType.MediaType == ContentTypes.Xrds) {
				return true;
			}

			if (response.Content.Headers.ContentType.MediaType == ContentTypes.Xml) {
				// This COULD be an XRDS document with an imprecise content-type.
				using (var responseStream = await response.Content.ReadAsStreamAsync()) {
					var readerSettings = MessagingUtilities.CreateUntrustedXmlReaderSettings();
					XmlReader reader = XmlReader.Create(responseStream, readerSettings);
					while (await reader.ReadAsync() && reader.NodeType != XmlNodeType.Element) {
						// intentionally blank
					}
					if (reader.NamespaceURI == XrdsNode.XrdsNamespace && reader.Name == "XRDS") {
						return true;
					}
				}
			}

			return false;
		}
	}
}
