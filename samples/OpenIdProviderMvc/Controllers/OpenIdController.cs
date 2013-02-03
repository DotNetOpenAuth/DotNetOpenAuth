namespace OpenIdProviderMvc.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Mvc.Ajax;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Behaviors;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.Provider.Behaviors;
	using OpenIdProviderMvc.Code;

	public class OpenIdController : Controller {
		internal static OpenIdProvider OpenIdProvider = new OpenIdProvider();

		public OpenIdController()
			: this(null) {
		}

		public OpenIdController(IFormsAuthentication formsAuthentication) {
			this.FormsAuth = formsAuthentication ?? new FormsAuthenticationService();
		}

		public IFormsAuthentication FormsAuth { get; private set; }

		[ValidateInput(false)]
		public ActionResult Provider() {
			IRequest request = OpenIdProvider.GetRequest();
			if (request != null) {
				// Some requests are automatically handled by DotNetOpenAuth.  If this is one, go ahead and let it go.
				if (request.IsResponseReady) {
					return OpenIdProvider.PrepareResponse(request).AsActionResult();
				}

				// This is apparently one that the host (the web site itself) has to respond to.
				ProviderEndpoint.PendingRequest = (IHostProcessedRequest)request;

				// If PAPE requires that the user has logged in recently, we may be required to challenge the user to log in.
				var papeRequest = ProviderEndpoint.PendingRequest.GetExtension<PolicyRequest>();
				if (papeRequest != null && papeRequest.MaximumAuthenticationAge.HasValue) {
					TimeSpan timeSinceLogin = DateTime.UtcNow - this.FormsAuth.SignedInTimestampUtc.Value;
					if (timeSinceLogin > papeRequest.MaximumAuthenticationAge.Value) {
						// The RP wants the user to have logged in more recently than he has.  
						// We'll have to redirect the user to a login screen.
						return this.RedirectToAction("LogOn", "Account", new { returnUrl = this.Url.Action("ProcessAuthRequest") });
					}
				}

				return this.ProcessAuthRequest();
			} else {
				// No OpenID request was recognized.  This may be a user that stumbled on the OP Endpoint.  
				return this.View();
			}
		}

		public ActionResult ProcessAuthRequest() {
			if (ProviderEndpoint.PendingRequest == null) {
				return this.RedirectToAction("Index", "Home");
			}

			// Try responding immediately if possible.
			ActionResult response;
			if (this.AutoRespondIfPossible(out response)) {
				return response;
			}

			// We can't respond immediately with a positive result.  But if we still have to respond immediately...
			if (ProviderEndpoint.PendingRequest.Immediate) {
				// We can't stop to prompt the user -- we must just return a negative response.
				return this.SendAssertion();
			}

			return this.RedirectToAction("AskUser");
		}

		/// <summary>
		/// Displays a confirmation page.
		/// </summary>
		/// <returns>The response for the user agent.</returns>
		[Authorize]
		public ActionResult AskUser() {
			if (ProviderEndpoint.PendingRequest == null) {
				// Oops... precious little we can confirm without a pending OpenID request.
				return this.RedirectToAction("Index", "Home");
			}

			// The user MAY have just logged in.  Try again to respond automatically to the RP if appropriate.
			ActionResult response;
			if (this.AutoRespondIfPossible(out response)) {
				return response;
			}

			if (!ProviderEndpoint.PendingAuthenticationRequest.IsDirectedIdentity &&
				!this.UserControlsIdentifier(ProviderEndpoint.PendingAuthenticationRequest)) {
				return this.Redirect(this.Url.Action("LogOn", "Account", new { returnUrl = this.Request.Url }));
			}

			this.ViewData["Realm"] = ProviderEndpoint.PendingRequest.Realm;

			return this.View();
		}

		[HttpPost, Authorize, ValidateAntiForgeryToken]
		public ActionResult AskUserResponse(bool confirmed) {
			if (!ProviderEndpoint.PendingAuthenticationRequest.IsDirectedIdentity &&
				!this.UserControlsIdentifier(ProviderEndpoint.PendingAuthenticationRequest))
			{
				// The user shouldn't have gotten this far without controlling the identifier we'd send an assertion for.
				return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
			}

			if (ProviderEndpoint.PendingAnonymousRequest != null) {
				ProviderEndpoint.PendingAnonymousRequest.IsApproved = confirmed;
			} else if (ProviderEndpoint.PendingAuthenticationRequest != null) {
				ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = confirmed;
			} else {
				throw new InvalidOperationException("There's no pending authentication request!");
			}

			return this.SendAssertion();
		}

		/// <summary>
		/// Sends a positive or a negative assertion, based on how the pending request is currently marked.
		/// </summary>
		/// <returns>An MVC redirect result.</returns>
		public ActionResult SendAssertion() {
			var pendingRequest = ProviderEndpoint.PendingRequest;
			var authReq = pendingRequest as IAuthenticationRequest;
			var anonReq = pendingRequest as IAnonymousRequest;
			ProviderEndpoint.PendingRequest = null; // clear session static so we don't do this again
			if (pendingRequest == null) {
				throw new InvalidOperationException("There's no pending authentication request!");
			}

			// Set safe defaults if somehow the user ended up (perhaps through XSRF) here before electing to send data to the RP.
			if (anonReq != null && !anonReq.IsApproved.HasValue) {
				anonReq.IsApproved = false;
			}

			if (authReq != null && !authReq.IsAuthenticated.HasValue) {
				authReq.IsAuthenticated = false;
			}

			if (authReq != null && authReq.IsAuthenticated.Value) {
				if (authReq.IsDirectedIdentity) {
					authReq.LocalIdentifier = Models.User.GetClaimedIdentifierForUser(User.Identity.Name);
				}

				if (!authReq.IsDelegatedIdentifier) {
					authReq.ClaimedIdentifier = authReq.LocalIdentifier;
				}
			}

			// Respond to AX/sreg extension requests only on a positive result.
			if ((authReq != null && authReq.IsAuthenticated.Value) ||
				(anonReq != null && anonReq.IsApproved.Value)) {
				// Look for a Simple Registration request.  When the AXFetchAsSregTransform behavior is turned on
				// in the web.config file as it is in this sample, AX requests will come in as SReg requests.
				var claimsRequest = pendingRequest.GetExtension<ClaimsRequest>();
				if (claimsRequest != null) {
					var claimsResponse = claimsRequest.CreateResponse();

					// This simple respond to a request check may be enhanced to only respond to an individual attribute
					// request if the user consents to it explicitly, in which case this response extension creation can take
					// place in the confirmation page action rather than here.
					if (claimsRequest.Email != DemandLevel.NoRequest) {
						claimsResponse.Email = User.Identity.Name + "@dotnetopenauth.net";
					}

					pendingRequest.AddResponseExtension(claimsResponse);
				}

				// Look for PAPE requests.
				var papeRequest = pendingRequest.GetExtension<PolicyRequest>();
				if (papeRequest != null) {
					var papeResponse = new PolicyResponse();
					if (papeRequest.MaximumAuthenticationAge.HasValue) {
						papeResponse.AuthenticationTimeUtc = this.FormsAuth.SignedInTimestampUtc;
					}

					pendingRequest.AddResponseExtension(papeResponse);
				}
			}

			return OpenIdProvider.PrepareResponse(pendingRequest).AsActionResult();
		}

		/// <summary>
		/// Attempts to formulate an automatic response to the RP if the user's profile allows it.
		/// </summary>
		/// <param name="response">Receives the ActionResult for the caller to return, or <c>null</c> if no automatic response can be made.</param>
		/// <returns>A value indicating whether an automatic response is possible.</returns>
		private bool AutoRespondIfPossible(out ActionResult response) {
			// If the odds are good we can respond to this one immediately (without prompting the user)...
			if (ProviderEndpoint.PendingRequest.IsReturnUrlDiscoverable(OpenIdProvider.Channel.WebRequestHandler) == RelyingPartyDiscoveryResult.Success
				&& User.Identity.IsAuthenticated
				&& this.HasUserAuthorizedAutoLogin(ProviderEndpoint.PendingRequest)) {
				// Is this is an identity authentication request? (as opposed to an anonymous request)...
				if (ProviderEndpoint.PendingAuthenticationRequest != null) {
					// If this is directed identity, or if the claimed identifier being checked is controlled by the current user...
					if (ProviderEndpoint.PendingAuthenticationRequest.IsDirectedIdentity
						|| this.UserControlsIdentifier(ProviderEndpoint.PendingAuthenticationRequest)) {
						ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = true;
						response = this.SendAssertion();
						return true;
					}
				}

				// If this is an anonymous request, we can respond to that too.
				if (ProviderEndpoint.PendingAnonymousRequest != null) {
					ProviderEndpoint.PendingAnonymousRequest.IsApproved = true;
					response = this.SendAssertion();
					return true;
				}
			}

			response = null;
			return false;
		}

		/// <summary>
		/// Determines whether the currently logged in user has authorized auto login to the requesting relying party.
		/// </summary>
		/// <param name="request">The incoming request.</param>
		/// <returns>
		/// 	<c>true</c> if it is safe to respond affirmatively to this request and all extensions
		/// 	without further user confirmation; otherwise, <c>false</c>.
		/// </returns>
		private bool HasUserAuthorizedAutoLogin(IHostProcessedRequest request) {
			// TODO: host should implement this method meaningfully, consulting their user database.
			// Make sure the user likes the RP
			if (true/*User.UserLikesRP(request.Realm))*/) {
				// And make sure the RP is only asking for information about the user that the user has granted before.
				if (true/*User.HasGrantedExtensions(request)*/) {
					// For now for the purposes of the sample, we'll disallow auto-logins when an sreg request is present.
					if (request.GetExtension<ClaimsRequest>() != null) {
						return false;
					}

					return true;
				}
			}

			// If we aren't sure the user likes this site and is willing to disclose the requested info, return false
			// so the user has the opportunity to explicity choose whether to share his/her info.
			return false;
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

			// Assuming the URLs on the web server are not case sensitive (on Windows servers they almost never are),
			// and usernames aren't either, compare the identifiers without case sensitivity.
			// No reason to do this for the PPID identifiers though, since they *can* be case sensitive and are highly
			// unlikely to be typed in by the user anyway.
			return string.Equals(authReq.LocalIdentifier.ToString(), userLocalIdentifier.ToString(), StringComparison.OrdinalIgnoreCase) ||
				authReq.LocalIdentifier == PpidGeneration.PpidIdentifierProvider.GetIdentifier(userLocalIdentifier, authReq.Realm);
		}
	}
}
