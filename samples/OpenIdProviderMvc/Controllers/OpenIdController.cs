namespace OpenIdProviderMvc.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Mvc.Ajax;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;
	using OpenIdProviderMvc.Code;

	public class OpenIdController : Controller {
		internal static OpenIdProvider OpenIdProvider = new OpenIdProvider();

		internal static IAuthenticationRequest PendingAuthenticationRequest {
			get { return ProviderEndpoint.PendingAuthenticationRequest; }
			set { ProviderEndpoint.PendingAuthenticationRequest = value; }
		}

		[ValidateInput(false)]
		public ActionResult PpidProvider() {
			return DoProvider(true);
		}

		[ValidateInput(false)]
		public ActionResult Provider() {
			return DoProvider(false);
		}

		[Authorize]
		public ActionResult SendAssertion(bool pseudonymous) {
			IAuthenticationRequest authReq = PendingAuthenticationRequest;
			PendingAuthenticationRequest = null;
			if (authReq == null) {
				throw new InvalidOperationException();
			}

			if (authReq.IsDirectedIdentity) {
				authReq.LocalIdentifier = Models.User.GetClaimedIdentifierForUser(User.Identity.Name);
				authReq.ClaimedIdentifier = authReq.LocalIdentifier;
				authReq.IsAuthenticated = true;
			} else {
				if (authReq.LocalIdentifier == Models.User.GetClaimedIdentifierForUser(User.Identity.Name)) {
					authReq.IsAuthenticated = true;
					if (!authReq.IsDelegatedIdentifier) {
						authReq.ClaimedIdentifier = authReq.LocalIdentifier;
					}
				} else {
					authReq.IsAuthenticated = false;
				}
			}

			if (pseudonymous) {
				var anonProvider = new AnonymousIdentifierProvider();
				authReq.ScrubPersonallyIdentifiableInformation(anonProvider, true);
			} else {
				// TODO: Respond to AX/sreg extension requests here
			}

			return OpenIdProvider.PrepareResponse(authReq).AsActionResult();
		}

		private ActionResult DoProvider(bool pseudonymous) {
			IRequest request = OpenIdProvider.GetRequest();
			if (request != null) {
				var authRequest = request as IAuthenticationRequest;
				if (authRequest != null) {
					PendingAuthenticationRequest = authRequest;
					if (User.Identity.IsAuthenticated && (authRequest.IsDirectedIdentity || Models.User.GetClaimedIdentifierForUser(User.Identity.Name) == authRequest.LocalIdentifier)) {
						return this.SendAssertion(pseudonymous);
					} else {
						return RedirectToAction("LogOn", "Account", new { returnUrl = Url.Action("SendAssertion", new { pseudonymous = pseudonymous }) });
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
	}
}
