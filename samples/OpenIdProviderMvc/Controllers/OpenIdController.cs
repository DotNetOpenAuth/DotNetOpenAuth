namespace OpenIdProviderMvc.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Mvc.Ajax;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using DotNetOpenAuth.OpenId.Provider;
	using OpenIdProviderMvc.Code;

	public class OpenIdController : Controller {
		internal static OpenIdProvider OpenIdProvider = new OpenIdProvider();

		private static AnonymousIdentifierProvider anonProvider = new AnonymousIdentifierProvider();

		internal static IAuthenticationRequest PendingAuthenticationRequest {
			get { return ProviderEndpoint.PendingAuthenticationRequest; }
			set { ProviderEndpoint.PendingAuthenticationRequest = value; }
		}

		[ValidateInput(false)]
		public ActionResult Provider() {
			IRequest request = OpenIdProvider.GetRequest();
			if (request != null) {
				var authRequest = request as IAuthenticationRequest;
				if (authRequest != null) {
					PendingAuthenticationRequest = authRequest;
					if (authRequest.IsReturnUrlDiscoverable(OpenIdProvider) == RelyingPartyDiscoveryResult.Success &&
						User.Identity.IsAuthenticated &&
						(authRequest.IsDirectedIdentity || Models.User.GetClaimedIdentifierForUser(User.Identity.Name) == authRequest.LocalIdentifier)) {
						return this.SendAssertion();
					} else {
						return RedirectToAction("LogOn", "Account", new { returnUrl = Url.Action("SendAssertion") });
					}
				}

				if (request.IsResponseReady) {
					return OpenIdProvider.PrepareResponse(request).AsActionResult();
				} else {
					return RedirectToAction("LogOn", "Account");
				}
			} else {
				return View();
			}
		}

		[Authorize]
		public ActionResult SendAssertion() {
			IAuthenticationRequest authReq = PendingAuthenticationRequest;
			PendingAuthenticationRequest = null;
			if (authReq == null) {
				throw new InvalidOperationException();
			}

			Identifier localIdentifier = Models.User.GetClaimedIdentifierForUser(User.Identity.Name);
			if (this.IsPpidRequested(authReq)) {
				if (!authReq.IsDirectedIdentity) {
					throw new InvalidOperationException("Directed identity is the only supported scenario for anonymous identifiers.");
				}

				var anonymousIdentifier = anonProvider.GetIdentifier(localIdentifier, authReq.Realm);
				authReq.ClaimedIdentifier = anonymousIdentifier;
				authReq.LocalIdentifier = anonymousIdentifier;
				authReq.IsAuthenticated = true;
			} else {
				if (authReq.IsDirectedIdentity) {
					authReq.LocalIdentifier = localIdentifier;
					authReq.ClaimedIdentifier = localIdentifier;
					authReq.IsAuthenticated = true;
				} else {
					if (authReq.LocalIdentifier == localIdentifier) {
						authReq.IsAuthenticated = true;
						if (!authReq.IsDelegatedIdentifier) {
							authReq.ClaimedIdentifier = authReq.LocalIdentifier;
						}
					} else {
						authReq.IsAuthenticated = false;
					}
				}

				// TODO: Respond to AX/sreg extension requests here.
				// We don't want to add these extension responses for anonymous identifiers
				// because they could leak information about the user's identity.
			}

			return OpenIdProvider.PrepareResponse(authReq).AsActionResult();
		}

		private bool IsPpidRequested(IAuthenticationRequest authRequest) {
			if (authRequest == null) {
				throw new ArgumentNullException("authRequest");
			}

			var pape = authRequest.GetExtension<PolicyRequest>();
			if (pape != null) {
				if (pape.PreferredPolicies.Contains(AuthenticationPolicies.PrivatePersonalIdentifier)) {
					return true;
				}
			}

			return false;
		}
	}
}
