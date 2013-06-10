//-----------------------------------------------------------------------
// <copyright file="OpenIdAjaxRelyingParty.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Net.Mime;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Script.Serialization;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Extensions.UI;
	using Validation;

	/// <summary>
	/// Provides the programmatic facilities to act as an AJAX-enabled OpenID relying party.
	/// </summary>
	public class OpenIdAjaxRelyingParty : OpenIdRelyingParty {
		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdAjaxRelyingParty"/> class.
		/// </summary>
		public OpenIdAjaxRelyingParty() {
			Reporting.RecordFeatureUse(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdAjaxRelyingParty"/> class.
		/// </summary>
		/// <param name="applicationStore">The application store.  If <c>null</c>, the relying party will always operate in "dumb mode".</param>
		public OpenIdAjaxRelyingParty(ICryptoKeyAndNonceStore applicationStore)
			: base(applicationStore) {
			Reporting.RecordFeatureUse(this);
		}

		/// <summary>
		/// Generates AJAX-ready authentication requests that can satisfy the requirements of some OpenID Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The Identifier supplied by the user.  This may be a URL, an XRI or i-name.</param>
		/// <param name="realm">The shorest URL that describes this relying party web site's address.
		/// For example, if your login page is found at https://www.example.com/login.aspx,
		/// your realm would typically be https://www.example.com/.</param>
		/// <param name="returnToUrl">The URL of the login page, or the page prepared to receive authentication
		/// responses from the OpenID Provider.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of authentication requests, any of which constitutes a valid identity assertion on the Claimed Identifier.
		/// Never null, but may be empty.
		/// </returns>
		/// <remarks>
		///   <para>Any individual generated request can satisfy the authentication.
		/// The generated requests are sorted in preferred order.
		/// Each request is generated as it is enumerated to.  Associations are created only as
		///   <see cref="IAuthenticationRequest.GetRedirectingResponseAsync" /> is called.</para>
		///   <para>No exception is thrown if no OpenID endpoints were discovered.
		/// An empty enumerable is returned instead.</para>
		/// </remarks>
		public override async Task<IEnumerable<IAuthenticationRequest>> CreateRequestsAsync(Identifier userSuppliedIdentifier, Realm realm, Uri returnToUrl, CancellationToken cancellationToken) {
			var requests = await base.CreateRequestsAsync(userSuppliedIdentifier, realm, returnToUrl, cancellationToken);
			var results = new List<IAuthenticationRequest>();

			// Alter the requests so that have AJAX characteristics.
			// Some OPs may be listed multiple times (one with HTTPS and the other with HTTP, for example).
			// Since we're gathering OPs to try one after the other, just take the first choice of each OP
			// and don't try it multiple times.
			requests = requests.Distinct(DuplicateRequestedHostsComparer.Instance);

			// Configure each generated request.
			int reqIndex = 0;
			foreach (var req in requests) {
				// Inform ourselves in return_to that we're in a popup.
				req.SetUntrustedCallbackArgument(OpenIdRelyingPartyControlBase.UIPopupCallbackKey, "1");

				if (req.DiscoveryResult.IsExtensionSupported<UIRequest>()) {
					// Inform the OP that we'll be using a popup window consistent with the UI extension.
					req.AddExtension(new UIRequest());

					// Provide a hint for the client javascript about whether the OP supports the UI extension.
					// This is so the window can be made the correct size for the extension.
					// If the OP doesn't advertise support for the extension, the javascript will use
					// a bigger popup window.
					req.SetUntrustedCallbackArgument(OpenIdRelyingPartyControlBase.PopupUISupportedJSHint, "1");
				}

				req.SetUntrustedCallbackArgument("index", (reqIndex++).ToString(CultureInfo.InvariantCulture));

				// If the ReturnToUrl was explicitly set, we'll need to reset our first parameter
				if (OpenIdElement.Configuration.RelyingParty.PreserveUserSuppliedIdentifier) {
					if (string.IsNullOrEmpty(HttpUtility.ParseQueryString(req.ReturnToUrl.Query)[AuthenticationRequest.UserSuppliedIdentifierParameterName])) {
						req.SetUntrustedCallbackArgument(AuthenticationRequest.UserSuppliedIdentifierParameterName, userSuppliedIdentifier.OriginalString);
					}
				}

				// Our javascript needs to let the user know which endpoint responded.  So we force it here.
				// This gives us the info even for 1.0 OPs and 2.0 setup_required responses.
				req.SetUntrustedCallbackArgument(OpenIdRelyingPartyAjaxControlBase.OPEndpointParameterName, req.Provider.Uri.AbsoluteUri);
				req.SetUntrustedCallbackArgument(OpenIdRelyingPartyAjaxControlBase.ClaimedIdParameterName, (string)req.ClaimedIdentifier ?? string.Empty);

				// Inform ourselves in return_to that we're in a popup or iframe.
				req.SetUntrustedCallbackArgument(OpenIdRelyingPartyAjaxControlBase.UIPopupCallbackKey, "1");

				// We append a # at the end so that if the OP happens to support it,
				// the OpenID response "query string" is appended after the hash rather than before, resulting in the
				// browser being super-speedy in closing the popup window since it doesn't try to pull a newer version
				// of the static resource down from the server merely because of a changed URL.
				// http://www.nabble.com/Re:-Defining-how-OpenID-should-behave-with-fragments-in-the-return_to-url-p22694227.html
				////TODO:

				results.Add(req);
			}

			return results;
		}

		/// <summary>
		/// Serializes discovery results on some <i>single</i> identifier on behalf of Javascript running on the browser.
		/// </summary>
		/// <param name="requests">The discovery results from just <i>one</i> identifier to serialize as a JSON response.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The JSON result to return to the user agent.
		/// </returns>
		/// <remarks>
		/// We prepare a JSON object with this interface:
		/// <code>
		/// class jsonResponse {
		///    string claimedIdentifier;
		///    Array requests; // never null
		///    string error; // null if no error
		/// }
		/// </code>
		/// Each element in the requests array looks like this:
		/// <code>
		/// class jsonAuthRequest {
		///    string endpoint;  // URL to the OP endpoint
		///    string immediate; // URL to initiate an immediate request
		///    string setup;     // URL to initiate a setup request.
		/// }
		/// </code>
		/// </remarks>
		public async Task<HttpResponseMessage> AsAjaxDiscoveryResultAsync(IEnumerable<IAuthenticationRequest> requests, CancellationToken cancellationToken) {
			Requires.NotNull(requests, "requests");

			var serializer = new JavaScriptSerializer();
			var response = new HttpResponseMessage {
				Content = new StringContent(serializer.Serialize(await this.AsJsonDiscoveryResultAsync(requests, cancellationToken))),
			};
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			return response;
		}

		/// <summary>
		/// Serializes discovery on a set of identifiers for preloading into an HTML page that carries
		/// an AJAX-aware OpenID control.
		/// </summary>
		/// <param name="requests">The discovery results to serialize as a JSON response.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The JSON result to return to the user agent.
		/// </returns>
		public async Task<string> AsAjaxPreloadedDiscoveryResultAsync(IEnumerable<IAuthenticationRequest> requests, CancellationToken cancellationToken) {
			Requires.NotNull(requests, "requests");

			var serializer = new JavaScriptSerializer();
			string json = serializer.Serialize(await this.AsJsonPreloadedDiscoveryResultAsync(requests, cancellationToken));

			string script = "window.dnoa_internal.loadPreloadedDiscoveryResults(" + json + ");";
			return script;
		}

		/// <summary>
		/// Converts a sequence of authentication requests to a JSON object for seeding an AJAX-enabled login page.
		/// </summary>
		/// <param name="requests">The discovery results from just <i>one</i> identifier to serialize as a JSON response.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A JSON object, not yet serialized.</returns>
		internal async Task<object> AsJsonDiscoveryResultAsync(IEnumerable<IAuthenticationRequest> requests, CancellationToken cancellationToken) {
			Requires.NotNull(requests, "requests");

			requests = requests.CacheGeneratedResults();

			if (requests.Any()) {
				var requestUrls =
					requests.Select(
						req =>
						new {
							endpoint = req.Provider.Uri.AbsoluteUri,
							immediate = this.GetRedirectUrlAsync(req, true, cancellationToken),
							setup = this.GetRedirectUrlAsync(req, false, cancellationToken),
						}).ToList();
				await Task.WhenAll(requestUrls.Select(r => r.immediate).Concat(requestUrls.Select(r => r.setup)));
				return new {
					claimedIdentifier = (string)requests.First().ClaimedIdentifier,
					requests = requestUrls.Select(req => new {
						endpoint = req.endpoint,
						immediate = req.immediate.Result, // await already took place
						setup = req.setup.Result, // await already took place
					}).ToArray()
				};
			} else {
				return new {
					requests = new object[0],
					error = OpenIdStrings.OpenIdEndpointNotFound,
				};
			}
		}

		/// <summary>
		/// Serializes discovery on a set of identifiers for preloading into an HTML page that carries
		/// an AJAX-aware OpenID control.
		/// </summary>
		/// <param name="requests">The discovery results to serialize as a JSON response.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A JSON object, not yet serialized to a string.
		/// </returns>
		private async Task<object> AsJsonPreloadedDiscoveryResultAsync(IEnumerable<IAuthenticationRequest> requests, CancellationToken cancellationToken) {
			Requires.NotNull(requests, "requests");

			// We prepare a JSON object with this interface:
			// Array discoveryWrappers;
			// Where each element in the above array has this interface:
			// class discoveryWrapper {
			//    string userSuppliedIdentifier;
			//    jsonResponse discoveryResult; // contains result of call to SerializeDiscoveryAsJson(Identifier)
			// }
			var jsonAsync = (
				from request in requests
				group request by request.DiscoveryResult.UserSuppliedIdentifier into requestsByIdentifier
				select new {
					userSuppliedIdentifier = (string)requestsByIdentifier.Key,
					discoveryResult = this.AsJsonDiscoveryResultAsync(requestsByIdentifier, cancellationToken),
				}).ToArray();
			await Task.WhenAll(jsonAsync.Select(j => j.discoveryResult));
			var json =
				from j in jsonAsync
				select new {
					userSuppliedIdentifier = j.userSuppliedIdentifier,
					discoveryResult = j.discoveryResult.Result, // await happened previously
				};
			return json;
		}

		/// <summary>
		/// Gets the full URL that carries an OpenID message, even if it exceeds the normal maximum size of a URL,
		/// for purposes of sending to an AJAX component running in the browser.
		/// </summary>
		/// <param name="request">The authentication request.</param>
		/// <param name="immediate"><c>true</c>to create a checkid_immediate request;
		/// <c>false</c> to create a checkid_setup request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The absolute URL that carries the entire OpenID message.
		/// </returns>
		private async Task<Uri> GetRedirectUrlAsync(IAuthenticationRequest request, bool immediate, CancellationToken cancellationToken) {
			Requires.NotNull(request, "request");

			request.Mode = immediate ? AuthenticationRequestMode.Immediate : AuthenticationRequestMode.Setup;
			var response = await request.GetRedirectingResponseAsync(cancellationToken);
			return response.GetDirectUriRequest();
		}
	}
}
