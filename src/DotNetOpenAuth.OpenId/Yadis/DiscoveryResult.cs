//-----------------------------------------------------------------------
// <copyright file="DiscoveryResult.cs" company="Scott Hanselman, Andrew Arnott">
//     Copyright (c) Scott Hanselman, Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Yadis {
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Net.Mime;
	using System.Threading.Tasks;
	using System.Web.UI.HtmlControls;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Contains the result of YADIS discovery.
	/// </summary>
	internal class DiscoveryResult {
		/// <summary>
		/// The original web response, backed up here if the final web response is the preferred response to use
		/// in case it turns out to not work out.
		/// </summary>
		private HttpResponseMessage htmlFallback;

		/// <summary>
		/// Prevents a default instance of the <see cref="DiscoveryResult" /> class from being created.
		/// </summary>
		private DiscoveryResult() {
		}

		/// <summary>
		/// Gets the URI of the original YADIS discovery request.  
		/// This is the user supplied Identifier as given in the original
		/// YADIS discovery request.
		/// </summary>
		public Uri RequestUri { get; private set; }

		/// <summary>
		/// Gets the fully resolved (after redirects) URL of the user supplied Identifier.
		/// This becomes the ClaimedIdentifier.
		/// </summary>
		public Uri NormalizedUri { get; private set; }

		/// <summary>
		/// Gets the location the XRDS document was downloaded from, if different
		/// from the user supplied Identifier.
		/// </summary>
		public Uri YadisLocation { get; private set; }

		/// <summary>
		/// Gets the Content-Type associated with the <see cref="ResponseText"/>.
		/// </summary>
		public MediaTypeHeaderValue ContentType { get; private set; }

		/// <summary>
		/// Gets the text in the final response.
		/// This may be an XRDS document or it may be an HTML document, 
		/// as determined by the <see cref="IsXrds"/> property.
		/// </summary>
		public string ResponseText { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the <see cref="ResponseText"/> 
		/// represents an XRDS document. False if the response is an HTML document.
		/// </summary>
		public bool IsXrds { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveryResult"/> class.
		/// </summary>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="initialResponse">The initial response.</param>
		/// <param name="finalResponse">The final response.</param>
		/// <returns>The newly initialized instance.</returns>
		internal static async Task<DiscoveryResult> CreateAsync(Uri requestUri, HttpResponseMessage initialResponse, HttpResponseMessage finalResponse) {
			var result = new DiscoveryResult();
			result.RequestUri = requestUri;
			result.NormalizedUri = initialResponse.RequestMessage.RequestUri;
			if (finalResponse == null || finalResponse.StatusCode != HttpStatusCode.OK) {
				await result.ApplyHtmlResponseAsync(initialResponse);
			} else {
				result.ContentType = finalResponse.Content.Headers.ContentType;
				result.ResponseText = await finalResponse.Content.ReadAsStringAsync();
				result.IsXrds = true;
				if (initialResponse != finalResponse) {
					result.YadisLocation = finalResponse.RequestMessage.RequestUri;
				}

				// Back up the initial HTML response in case the XRDS is not useful.
				result.htmlFallback = initialResponse;
			}

			return result;
		}

		/// <summary>
		/// Reverts to the HTML response after the XRDS response didn't work out.
		/// </summary>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		internal async Task TryRevertToHtmlResponseAsync() {
			if (this.htmlFallback != null) {
				await this.ApplyHtmlResponseAsync(this.htmlFallback);
				this.htmlFallback = null;
			}
		}

		/// <summary>
		/// Applies the HTML response to the object.
		/// </summary>
		/// <param name="response">The initial response.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		private async Task ApplyHtmlResponseAsync(HttpResponseMessage response) {
			Requires.NotNull(response, "response");

			this.ContentType = response.Content.Headers.ContentType;
			this.ResponseText = await response.Content.ReadAsStringAsync();
			this.IsXrds = this.ContentType != null && this.ContentType.MediaType == ContentTypes.Xrds;
		}
	}
}
