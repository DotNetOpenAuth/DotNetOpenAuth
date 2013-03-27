//-----------------------------------------------------------------------
// <copyright file="AutoRedirectHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Messaging;

	internal class AutoRedirectHandler : DelegatingHandler {
		internal AutoRedirectHandler(HttpMessageHandler innerHandler)
			: base(innerHandler) {
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken) {
			HttpResponseMessage response = null;
			do {
				if (response != null) {
					var modifiedRequest = MessagingUtilities.Clone(request);
					modifiedRequest.RequestUri = new Uri(request.RequestUri, response.Headers.Location);
					request = modifiedRequest;
				}

				response = await base.SendAsync(request, cancellationToken);
			}
			while (response.StatusCode == HttpStatusCode.Redirect);

			return response;
		}
	}
}
