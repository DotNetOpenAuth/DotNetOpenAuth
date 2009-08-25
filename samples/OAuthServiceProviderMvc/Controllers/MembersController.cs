using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.Messages;

namespace oAuthMVC.Controllers
{
    [HandleError]
    [Authorize]
    public class MembersController : Controller
    {
        private static readonly RandomNumberGenerator CryptoRandomDataGenerator = new RNGCryptoServiceProvider();

        private string AuthorizationSecret
        {
            get { return Session["OAuthAuthorizationSecret"] as string; }
            set { Session["OAuthAuthorizationSecret"] = value; }
        }

        public ActionResult Authorize()
        {
            if (String.IsNullOrEmpty(Request.Form["allow"]))
            {
                if (Global.PendingOAuthAuthorization == null)
                {
                    return RedirectToAction("AuthorizedConsumers");
                }
                else
                {
                    ITokenContainingMessage pendingToken = Global.PendingOAuthAuthorization;
                    var token = Global.DataContext.OAuthTokens.Single(t => t.Token == pendingToken.Token);
                    ViewData["Scope"] = token.Scope;
                    ViewData["ConsumerKey"] = Global.TokenManager.GetConsumerForToken(token.Token).ConsumerKey;

                    // Generate an unpredictable secret that goes to the user agent and must come back
                    // with authorization to guarantee the user interacted with this page rather than
                    // being scripted by an evil Consumer.
                    byte[] randomData = new byte[8];
                    CryptoRandomDataGenerator.GetBytes(randomData);
                    this.AuthorizationSecret = Convert.ToBase64String(randomData);
                    TempData["OAuthAuthorizationSecToken"] = this.AuthorizationSecret;

                    ViewData["OAuth10ConsumerWarningVisible"] = Global.PendingOAuthAuthorization.IsUnsafeRequest;
                }
            }
            else if (Request.Form["allow"] == "1") //allow
            {
                ViewData["ActiveIndex"] = "1";

                if (this.AuthorizationSecret != Request.Form["OAuthAuthorizationSecToken"])
                {
                    throw new ArgumentException(); // probably someone trying to hack in.
                }
                this.AuthorizationSecret = null; // clear one time use secret
                var pending = Global.PendingOAuthAuthorization;
                Global.AuthorizePendingRequestToken();

                ServiceProvider sp = new ServiceProvider(Constants.SelfDescription, Global.TokenManager);
                var response = sp.PrepareAuthorizationResponse(pending);
                if (response != null)
                {
                    sp.Channel.Send(response);
                }
                else
                {
                    if (pending.IsUnsafeRequest)
                    {
                        ViewData["VerifierMultiView"] = "1";
                    }
                    else
                    {
                        string verifier = ServiceProvider.CreateVerificationCode(VerificationCodeFormat.AlphaNumericNoLookAlikes, 10);
                        ViewData["VerificationCode"] = verifier;
                        ITokenContainingMessage requestTokenMessage = pending;
                        Global.TokenManager.GetRequestToken(requestTokenMessage.Token).VerificationCode = verifier;
                    }
                }
            }
            else
            {
                // erase the request token.
                ViewData["ActiveIndex"] = "2";
            }

            return View();
        }

        public ActionResult AuthorizedConsumers()
        {
            return View();
        }

        public ActionResult Logoff()
        {
            return View();
        }

        public ActionResult Allow()
        {
            return View();
        }

        public ActionResult Deny()
        {
            return View();
        }
    }
}
