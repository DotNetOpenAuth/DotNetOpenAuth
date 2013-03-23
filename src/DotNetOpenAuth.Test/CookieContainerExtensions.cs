//-----------------------------------------------------------------------
// <copyright file="CookieContainerExtensions.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Http;

	using Validation;

	internal static class CookieContainerExtensions {
		internal static void SetCookies(this CookieContainer container, HttpResponseMessage response, Uri requestUri = null) {
			Requires.NotNull(container, "container");
			Requires.NotNull(response, "response");

			IEnumerable<string> cookieHeaders;
			if (response.Headers.TryGetValues("Set-Cookie", out cookieHeaders)) {
				foreach (string cookie in cookieHeaders) {
					container.SetCookies(requestUri ?? response.RequestMessage.RequestUri, cookie);
				}
			}
		}

		internal static void ApplyCookies(this CookieContainer container, HttpRequestMessage request) {
			Requires.NotNull(container, "container");
			Requires.NotNull(request, "request");

			string cookieHeader = container.GetCookieHeader(request.RequestUri);
			if (!string.IsNullOrEmpty(cookieHeader)) {
				request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
			}
		}
	}
}