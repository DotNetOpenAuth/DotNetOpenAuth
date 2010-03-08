namespace MvcRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public interface IOpenIdRelyingParty {
		Channel Channel { get; }

		IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy);

		IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy);

		ActionResult AjaxDiscovery(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy);

		ActionResult ProcessAjaxOpenIdResponse();

		IAuthenticationResponse GetResponse();

		IAuthenticationResponse GetResponse(HttpRequestInfo request);
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

		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy) {
			return this.CreateRequests(userSuppliedIdentifier, realm, returnTo, privacyPolicy).First();
		}

		public IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy) {
			if (userSuppliedIdentifier == null) {
				throw new ArgumentNullException("userSuppliedIdentifier");
			}
			if (realm == null) {
				throw new ArgumentNullException("realm");
			}
			if (returnTo == null) {
				throw new ArgumentNullException("returnTo");
			}

			var requests = relyingParty.CreateRequests(userSuppliedIdentifier, realm, returnTo);

			foreach (IAuthenticationRequest request in requests) {
				// Ask for the user's email, not because we necessarily need it to do our work,
				// but so we can display something meaningful to the user as their "username"
				// when they log in with a PPID from Google, for example.
				request.AddExtension(new ClaimsRequest {
					Email = DemandLevel.Require,
					FullName = DemandLevel.Request,
					PolicyUrl = privacyPolicy,
				});

				yield return request;
			}
		}

		public ActionResult AjaxDiscovery(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy) {
			return relyingParty.AsAjaxDiscoveryResult(
				this.CreateRequests(userSuppliedIdentifier, realm, returnTo, privacyPolicy)).AsActionResult();
		}

		public ActionResult ProcessAjaxOpenIdResponse() {
			return relyingParty.ProcessAjaxOpenIdResponse().AsActionResult();
		}

		public IAuthenticationResponse GetResponse() {
			return relyingParty.GetResponse();
		}

		public IAuthenticationResponse GetResponse(HttpRequestInfo request) {
			return relyingParty.GetResponse(request);
		}

		#endregion
	}
}
