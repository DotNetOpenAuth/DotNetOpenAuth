namespace OpenIdProviderMvc.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Mvc.Ajax;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Behaviors;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using DotNetOpenAuth.OpenId.Provider;
	using OpenIdProviderMvc.Code;

	public class OpenIdController : Controller {
		internal static OpenIdProvider OpenIdProvider = new OpenIdProvider();

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
						(authRequest.IsDirectedIdentity || this.UserControlsIdentifier(authRequest))) {
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
			PendingAuthenticationRequest = null; // clear session static so we don't do this again
			if (authReq == null) {
				throw new InvalidOperationException("There's no pending authentication request!");
			}

			if (authReq.IsDirectedIdentity) {
				authReq.LocalIdentifier = Models.User.GetClaimedIdentifierForUser(User.Identity.Name);
			}
			if (!authReq.IsDelegatedIdentifier) {
				authReq.ClaimedIdentifier = authReq.LocalIdentifier;
			}

			// Respond to AX/sreg extension requests.
			//// Real web sites would have code here

			authReq.IsAuthenticated = this.UserControlsIdentifier(authReq);
			return OpenIdProvider.PrepareResponse(authReq).AsActionResult();
		}

		/// <summary>
		/// Checks whether the logged in user controls the OP local identifier in the given authentication request.
		/// </summary>
		/// <param name="authReq">The authentication request.</param>
		/// <returns><c>true</c> if the user controls the identifier; <c>false</c> otherwise.</returns>
		private bool UserControlsIdentifier(IAuthenticationRequest authReq) {
			if (authReq == null) {
				throw new ArgumentNullException("authReq");
			}

			if (User == null || User.Identity == null) {
				return false;
			}

			Uri userLocalIdentifier = Models.User.GetClaimedIdentifierForUser(User.Identity.Name);
			return authReq.LocalIdentifier == userLocalIdentifier ||
				authReq.LocalIdentifier == PpidGeneration.PpidIdentifierProvider.GetIdentifier(userLocalIdentifier, authReq.Realm);
		}
	}
}
