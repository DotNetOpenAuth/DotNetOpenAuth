using MvcRelyingParty.Code;

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
		[OAuthAuthorize]
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
		[OAuthAuthorize, AcceptVerbs(HttpVerbs.Put), ValidateAntiForgeryToken]
		public ActionResult Update(string firstName, string lastName, string emailAddress) {
			Database.LoggedInUser.FirstName = firstName;
			Database.LoggedInUser.LastName = lastName;

			if (Database.LoggedInUser.EmailAddress != emailAddress) {
				Database.LoggedInUser.EmailAddress = emailAddress;
				Database.LoggedInUser.EmailAddressVerified = false;
			}

			return PartialView("EditFields", GetAccountInfoModel());
		}

		[Authorize]
		[OAuthUserAuthorizationEndpoint]
		[ActionName("authorize")]
		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult Authorize() {
			if (OAuthServiceProvider.PendingAuthorizationRequest == null) {
				return RedirectToAction("Edit");
			}

			var model = new AccountAuthorizeModel {
				ConsumerApp = OAuthServiceProvider.PendingAuthorizationConsumer.Name,
				IsUnsafeRequest = OAuthServiceProvider.PendingAuthorizationRequest.IsUnsafeRequest,
			};

			return View(model);
		}

		[Authorize, AcceptVerbs(HttpVerbs.Post), ValidateAntiForgeryToken]
		public ActionResult Authorize(bool isApproved) {
			if (isApproved) {
				var consumer = OAuthServiceProvider.PendingAuthorizationConsumer;
				var tokenManager = OAuthServiceProvider.ServiceProvider.TokenManager;
				var pendingRequest = OAuthServiceProvider.PendingAuthorizationRequest;
				ITokenContainingMessage requestTokenMessage = pendingRequest;
				var requestToken = tokenManager.GetRequestToken(requestTokenMessage.Token);

				var response = OAuthServiceProvider.AuthorizePendingRequestTokenAsWebResponse();
				if (response != null) {
					// The consumer provided a callback URL that can take care of everything else.
					return response.AsActionResult();
				}

				var model = new AccountAuthorizeModel {
					ConsumerApp = consumer.Name,
				};

				if (!pendingRequest.IsUnsafeRequest) {
					model.VerificationCode = ServiceProvider.CreateVerificationCode(consumer.VerificationCodeFormat, consumer.VerificationCodeLength);
					requestToken.VerificationCode = model.VerificationCode;
					tokenManager.UpdateToken(requestToken);
				}

				return View("AuthorizeApproved", model);
			} else {
				OAuthServiceProvider.PendingAuthorizationRequest = null;
				return View("AuthorizeDenied");
			}
		}

		[Authorize, AcceptVerbs(HttpVerbs.Delete)] // ValidateAntiForgeryToken would be GREAT here, but it's not a FORM POST operation so that doesn't work.
		public ActionResult RevokeToken(string token) {
			if (String.IsNullOrEmpty(token)) {
				throw new ArgumentNullException("token");
			}

			var tokenEntity = Database.DataContext.IssuedTokens.OfType<IssuedAccessToken>().Where(t => t.User.UserId == Database.LoggedInUser.UserId && t.Token == token).FirstOrDefault();
			if (tokenEntity == null) {
				throw new ArgumentOutOfRangeException("id", "The logged in user does not have a token with this name to revoke.");
			}

			Database.DataContext.DeleteObject(tokenEntity);
			Database.DataContext.SaveChanges(); // make changes now so the model we fill up reflects the change

			return PartialView("AuthorizedApps", GetAccountInfoModel());
		}

		[OAuthRequestTokenEndpoint]
		[AcceptVerbs(HttpVerbs.Post)]
		[ActionName("request_token")]
		public ActionResult GetRequestToken()
		{
			var serviceProvider = OAuthServiceProvider.ServiceProvider;
			var requestMessage = serviceProvider.ReadTokenRequest();
			var response = serviceProvider.PrepareUnauthorizedTokenMessage(requestMessage);
			return serviceProvider.Channel.PrepareResponse(response).AsActionResult();
		}

		[OAuthAccessTokenEndpoint]
		[ActionName("access_token")]
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult GetAccessToken() {
			var serviceProvider = OAuthServiceProvider.ServiceProvider;
			var requestMessage = serviceProvider.ReadAccessTokenRequest();
			var response = serviceProvider.PrepareAccessTokenMessage(requestMessage);
			return serviceProvider.Channel.PrepareResponse(response).AsActionResult();
		}

		private static AccountInfoModel GetAccountInfoModel() {
			var authorizedApps = from token in Database.DataContext.IssuedTokens.OfType<IssuedAccessToken>()
								 where token.User.UserId == Database.LoggedInUser.UserId
								 select new AccountInfoModel.AuthorizedApp { AppName = token.Consumer.Name, Token = token.Token };
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
