namespace MvcRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Mvc;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Validation;

	public interface IOpenIdRelyingParty {
		Channel Channel { get; }

		Task<IAuthenticationRequest> CreateRequestAsync(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy, CancellationToken cancellationToken = default(CancellationToken));

		Task<IEnumerable<IAuthenticationRequest>> CreateRequestsAsync(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy, CancellationToken cancellationToken = default(CancellationToken));

		Task<ActionResult> AjaxDiscoveryAsync(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy, CancellationToken cancellationToken = default(CancellationToken));

		Task<string> PreloadDiscoveryResultsAsync(Realm realm, Uri returnTo, Uri privacyPolicy, CancellationToken cancellationToken = default(CancellationToken), params Identifier[] identifiers);

		Task<ActionResult> ProcessAjaxOpenIdResponseAsync(HttpRequestBase request, CancellationToken cancellationToken = default(CancellationToken));

		Task<IAuthenticationResponse> GetResponseAsync(HttpRequestBase request, CancellationToken cancellationToken = default(CancellationToken));
	}

	/// <summary>
	/// A wrapper around the standard <see cref="OpenIdRelyingParty"/> class.
	/// </summary>
	public class OpenIdRelyingPartyService : IOpenIdRelyingParty {
		/// <summary>
		/// The OpenID relying party to use for logging users in.
		/// </summary>
		/// <remarks>
		/// This is static because it is thread-safe and is more expensive
		/// to create than we want to run through for every single page request.
		/// </remarks>
		private static OpenIdAjaxRelyingParty relyingParty = new OpenIdAjaxRelyingParty();

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingPartyService"/> class.
		/// </summary>
		public OpenIdRelyingPartyService() {
		}

		#region IOpenIdRelyingParty Members

		public Channel Channel {
			get { return relyingParty.Channel; }
		}

		public async Task<IAuthenticationRequest> CreateRequestAsync(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy, CancellationToken cancellationToken = default(CancellationToken)) {
			return (await this.CreateRequestsAsync(userSuppliedIdentifier, realm, returnTo, privacyPolicy, cancellationToken)).First();
		}

		public async Task<IEnumerable<IAuthenticationRequest>> CreateRequestsAsync(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(userSuppliedIdentifier, "userSuppliedIdentifier");
			Requires.NotNull(realm, "realm");
			Requires.NotNull(returnTo, "returnTo");

			var requests = (await relyingParty.CreateRequestsAsync(userSuppliedIdentifier, realm, returnTo, cancellationToken)).ToList();

			foreach (IAuthenticationRequest request in requests) {
				// Ask for the user's email, not because we necessarily need it to do our work,
				// but so we can display something meaningful to the user as their "username"
				// when they log in with a PPID from Google, for example.
				request.AddExtension(new ClaimsRequest {
					Email = DemandLevel.Require,
					FullName = DemandLevel.Request,
					PolicyUrl = privacyPolicy,
				});
			}

			return requests;
		}

		public async Task<ActionResult> AjaxDiscoveryAsync(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy, CancellationToken cancellationToken = default(CancellationToken)) {
			return (await relyingParty.AsAjaxDiscoveryResultAsync(
				await this.CreateRequestsAsync(userSuppliedIdentifier, realm, returnTo, privacyPolicy, cancellationToken),
				cancellationToken)).AsActionResult();
		}

		public async Task<string> PreloadDiscoveryResultsAsync(Realm realm, Uri returnTo, Uri privacyPolicy, CancellationToken cancellationToken = default(CancellationToken), params Identifier[] identifiers) {
			var results = new List<IAuthenticationRequest>();
			foreach (var id in identifiers) {
				var discoveryResult = await this.CreateRequestsAsync(id, realm, returnTo, privacyPolicy, cancellationToken);
				results.AddRange(discoveryResult);
			}

			return await relyingParty.AsAjaxPreloadedDiscoveryResultAsync(results, cancellationToken);
		}

		public async Task<ActionResult> ProcessAjaxOpenIdResponseAsync(HttpRequestBase request, CancellationToken cancellationToken = default(CancellationToken)) {
			return (await relyingParty.ProcessResponseFromPopupAsync(request, cancellationToken)).AsActionResult();
		}

		public Task<IAuthenticationResponse> GetResponseAsync(HttpRequestBase request, CancellationToken cancellationToken = default(CancellationToken)) {
			return relyingParty.GetResponseAsync(request, cancellationToken);
		}

		#endregion
	}
}
