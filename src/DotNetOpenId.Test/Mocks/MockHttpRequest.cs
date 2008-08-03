using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using NUnit.Framework;

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
				Assert.Fail("Unexpected HTTP request: {0}", uri);
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
			registeredMockResponses.Add(response.RequestUri, response);
		}

		internal static void RegisterMockResponse(Uri uri, string contentType, string responseBody) {
			string contentEncoding = null;
			MemoryStream stream = new MemoryStream();
			StreamWriter sw = new StreamWriter(stream);
			sw.Write(responseBody);
			sw.Flush();
			stream.Seek(0, SeekOrigin.Begin);
			RegisterMockResponse(new UntrustedWebResponse(uri, uri, new WebHeaderCollection(),
				HttpStatusCode.OK, contentType, contentEncoding, stream));
		}

		internal static void RegisterMockXrdsResponses(IDictionary<string, string> requestUriAndResponseBody) {
			foreach (var pair in requestUriAndResponseBody) {
				RegisterMockResponse(new Uri(pair.Key), "text/xml; saml=false; https=false; charset=UTF-8", pair.Value);
			}
		}
	}
}
