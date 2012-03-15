namespace MvcRelyingParty.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Security;
	using System.Web.UI;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using MvcRelyingParty.Models;
	using RelyingPartyLogic;

	[HandleError]
	public class AccountController : Controller {
		[Authorize]
		public ActionResult Edit() {
			return View(GetAccountInfoModel());
		}

		/// <summary>
		/// Updates the user's account information.
		/// </summary>
		/// <param name="firstName">The first name.</param>
		/// <param name="lastName">The last name.</param>
		/// <param name="emailAddress">The email address.</param>
		/// <returns>An updated view showing the new profile.</returns>
		/// <remarks>
		/// This action accepts PUT because this operation is idempotent in nature.
		/// </remarks>
		[Authorize, AcceptVerbs(HttpVerbs.Put), ValidateAntiForgeryToken]
		public ActionResult Update(string firstName, string lastName, string emailAddress) {
			Database.LoggedInUser.FirstName = firstName;
			Database.LoggedInUser.LastName = lastName;

			if (Database.LoggedInUser.EmailAddress != emailAddress) {
				Database.LoggedInUser.EmailAddress = emailAddress;
				Database.LoggedInUser.EmailAddressVerified = false;
			}

			return PartialView("EditFields", GetAccountInfoModel());
		}

		[Authorize, AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
		[HttpHeader("x-frame-options", "SAMEORIGIN")] // mitigates clickjacking
		public ActionResult Authorize() {
			var pendingRequest = OAuthServiceProvider.AuthorizationServer.ReadAuthorizationRequest();
			if (pendingRequest == null) {
				throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
			}

			var requestingClient = Database.DataContext.Clients.First(c => c.ClientIdentifier == pendingRequest.ClientIdentifier);

			// Consider auto-approving if safe to do so.
			if (((OAuthAuthorizationServer)OAuthServiceProvider.AuthorizationServer.AuthorizationServerServices).CanBeAutoApproved(pendingRequest)) {
				var approval = OAuthServiceProvider.AuthorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, HttpContext.User.Identity.Name);
				return OAuthServiceProvider.AuthorizationServer.Channel.PrepareResponse(approval).AsActionResult();
			}

			var model = new AccountAuthorizeModel {
				ClientApp = requestingClient.Name,
				Scope = pendingRequest.Scope,
				AuthorizationRequest = pendingRequest,
			};

			return View(model);
		}

		[Authorize, AcceptVerbs(HttpVerbs.Post), ValidateAntiForgeryToken]
		public ActionResult AuthorizeResponse(bool isApproved) {
			var pendingRequest = OAuthServiceProvider.AuthorizationServer.ReadAuthorizationRequest();
			if (pendingRequest == null) {
				throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
			}
			var requestingClient = Database.DataContext.Clients.First(c => c.ClientIdentifier == pendingRequest.ClientIdentifier);

			IDirectedProtocolMessage response;
			if (isApproved) {
				Database.LoggedInUser.ClientAuthorizations.Add(
					new ClientAuthorization() {
						Client = requestingClient,
						Scope = string.Join(" ", pendingRequest.Scope.ToArray()),
						User = Database.LoggedInUser,
						CreatedOnUtc = DateTime.UtcNow.CutToSecond(),
					});
				response = OAuthServiceProvider.AuthorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, HttpContext.User.Identity.Name);
			} else {
				response = OAuthServiceProvider.AuthorizationServer.PrepareRejectAuthorizationRequest(pendingRequest);
			}

			return OAuthServiceProvider.AuthorizationServer.Channel.PrepareResponse(response).AsActionResult();
		}

		[Authorize, AcceptVerbs(HttpVerbs.Delete)] // ValidateAntiForgeryToken would be GREAT here, but it's not a FORM POST operation so that doesn't work.
		public ActionResult RevokeAuthorization(int authorizationId) {
			var tokenEntity = Database.DataContext.ClientAuthorizations.Where(auth => auth.User.UserId == Database.LoggedInUser.UserId && auth.AuthorizationId == authorizationId).FirstOrDefault();
			if (tokenEntity == null) {
				throw new ArgumentOutOfRangeException("id", "The logged in user does not have a token with this name to revoke.");
			}

			Database.DataContext.DeleteObject(tokenEntity);
			Database.DataContext.SaveChanges(); // make changes now so the model we fill up reflects the change

			return PartialView("AuthorizedApps", GetAccountInfoModel());
		}

		private static AccountInfoModel GetAccountInfoModel() {
			var authorizedApps = from auth in Database.DataContext.ClientAuthorizations
								 where auth.User.UserId == Database.LoggedInUser.UserId
								 select new AccountInfoModel.AuthorizedApp { AppName = auth.Client.Name, AuthorizationId = auth.AuthorizationId, Scope = auth.Scope };
			Database.LoggedInUser.AuthenticationTokens.Load();
			var model = new AccountInfoModel {
				FirstName = Database.LoggedInUser.FirstName,
				LastName = Database.LoggedInUser.LastName,
				EmailAddress = Database.LoggedInUser.EmailAddress,
				AuthorizedApps = authorizedApps.ToList(),
				AuthenticationTokens = Database.LoggedInUser.AuthenticationTokens.ToList(),
			};
			return model;
		}
	}
}
