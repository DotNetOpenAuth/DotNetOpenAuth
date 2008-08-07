using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Yadis;
using NUnit.Framework;
using System.Diagnostics;
using System.Web;

namespace DotNetOpenId.Test.Mocks {
	class MockHttpRequest {
		static Dictionary<Uri, UntrustedWebResponse> registeredMockResponses = new Dictionary<Uri, UntrustedWebResponse>();

		static UntrustedWebResponse MockRequestResponse(Uri uri, byte[] body, string[] acceptTypes) {
			UntrustedWebResponse response;
			if (registeredMockResponses.TryGetValue(uri, out response)) {
				// reset response stream position so this response can be reused on a subsequent request.
				response.ResponseStream.Seek(0, SeekOrigin.Begin);
				return response;
			} else {
				//Assert.Fail("Unexpected HTTP request: {0}", uri);
				return new UntrustedWebResponse(uri, uri, new WebHeaderCollection(), HttpStatusCode.NotFound,
					"text/html", null, new MemoryStream());
			}
		}

		/// <summary>
		/// Clears all all mock HTTP responses and deactivates HTTP mocking.
		/// </summary>
		internal static void Reset() {
			UntrustedWebRequest.MockRequests = null;
			registeredMockResponses.Clear();
		}

		internal static void RegisterMockResponse(UntrustedWebResponse response) {
			if (response == null) throw new ArgumentNullException("response");
			UntrustedWebRequest.MockRequests = MockRequestResponse;
			if (registeredMockResponses.ContainsKey(response.RequestUri)) {
				Trace.TraceWarning("Mock HTTP response already registered for {0}.", response.RequestUri);
			} else {
				registeredMockResponses.Add(response.RequestUri, response);
			}
		}

		internal static void RegisterMockResponse(Uri requestUri, string contentType, string responseBody) {
			RegisterMockResponse(requestUri, requestUri, contentType, responseBody);
		}

		internal static void RegisterMockResponse(Uri requestUri, Uri responseUri, string contentType, string responseBody) {
			RegisterMockResponse(requestUri, responseUri, contentType, new WebHeaderCollection(), responseBody);
		}

		internal static void RegisterMockResponse(Uri requestUri, Uri responseUri, string contentType, WebHeaderCollection headers, string responseBody) {
			if (requestUri == null) throw new ArgumentNullException("requestUri");
			if (responseUri == null) throw new ArgumentNullException("responseUri");
			if (String.IsNullOrEmpty(contentType)) throw new ArgumentNullException("contentType");

			string contentEncoding = null;
			MemoryStream stream = new MemoryStream();
			StreamWriter sw = new StreamWriter(stream);
			sw.Write(responseBody);
			sw.Flush();
			stream.Seek(0, SeekOrigin.Begin);
			RegisterMockResponse(new UntrustedWebResponse(requestUri, responseUri, headers ?? new WebHeaderCollection(),
				HttpStatusCode.OK, contentType, contentEncoding, stream));
		}

		internal static void RegisterMockXrdsResponses(IDictionary<string, string> requestUriAndResponseBody) {
			foreach (var pair in requestUriAndResponseBody) {
				RegisterMockResponse(new Uri(pair.Key), "text/xml; saml=false; https=false; charset=UTF-8", pair.Value);
			}
		}

		internal static void RegisterMockXrdsResponse(ServiceEndpoint endpoint) {
			if (endpoint == null) throw new ArgumentNullException("endpoint");

			string template = @"<xrds:XRDS xmlns:xrds='xri://$xrds' xmlns:openid='http://openid.net/xmlns/1.0' xmlns='xri://$xrd*($v*2.0)'>
	<XRD>
		<Service priority='10'>
			<Type>{0}</Type>
			<URI>{1}</URI>
			<LocalID>{2}</LocalID>
			<openid:Delegate xmlns:openid='http://openid.net/xmlns/1.0'>{2}</openid:Delegate>
		</Service>
	</XRD>
</xrds:XRDS>";
			string serviceTypeUri, identityUri;
			if (endpoint.ClaimedIdentifier == endpoint.Protocol.ClaimedIdentifierForOPIdentifier) {
				serviceTypeUri = endpoint.Protocol.OPIdentifierServiceTypeURI;
				identityUri = endpoint.UserSuppliedIdentifier;
			} else {
				serviceTypeUri = endpoint.Protocol.ClaimedIdentifierServiceTypeURI;
				identityUri = endpoint.UserSuppliedIdentifier ?? endpoint.ClaimedIdentifier;
			}
			string xrds = string.Format(CultureInfo.InvariantCulture, template, 
				HttpUtility.HtmlEncode(serviceTypeUri), 
				HttpUtility.HtmlEncode(endpoint.ProviderEndpoint.AbsoluteUri), 
				HttpUtility.HtmlEncode(endpoint.ProviderLocalIdentifier)
				);

			RegisterMockResponse(new Uri(identityUri), ContentTypes.Xrds, xrds);
		}
		internal static void RegisterMockXrdsResponse(UriIdentifier directedIdentityAssignedIdentifier, ServiceEndpoint providerEndpoint) {
			ServiceEndpoint identityEndpoint = ServiceEndpoint.CreateForClaimedIdentifier(
				directedIdentityAssignedIdentifier,
				directedIdentityAssignedIdentifier,
				providerEndpoint.ProviderEndpoint,
				new string[] { providerEndpoint.Protocol.ClaimedIdentifierServiceTypeURI },
				10,
				10
				);
			RegisterMockXrdsResponse(identityEndpoint);
		}
		internal static Identifier RegisterMockXrdsResponse(string embeddedResourcePath) {
			UriIdentifier id = TestSupport.GetFullUrl(embeddedResourcePath);
			RegisterMockResponse(id, "application/xrds+xml", TestSupport.LoadEmbeddedFile(embeddedResourcePath));
			return id;
		}
		internal static void RegisterMockRPDiscovery() {
			Uri rpRealmUri = TestSupport.Realm.UriWithWildcardChangedToWww;

			string template = @"<xrds:XRDS xmlns:xrds='xri://$xrds' xmlns:openid='http://openid.net/xmlns/1.0' xmlns='xri://$xrd*($v*2.0)'>
	<XRD>
		<Service priority='10'>
			<Type>{0}</Type>
			<URI>{1}</URI>
		</Service>
	</XRD>
</xrds:XRDS>";
			string xrds = string.Format(CultureInfo.InvariantCulture, template,
				HttpUtility.HtmlEncode(Protocol.v20.RPReturnToTypeURI),
				HttpUtility.HtmlEncode(rpRealmUri.AbsoluteUri)
				);

			RegisterMockResponse(rpRealmUri, ContentTypes.Xrds, xrds);
		}

		internal static void DeleteResponse(Uri requestUri) {
			registeredMockResponses.Remove(requestUri);
		}
	}
}
