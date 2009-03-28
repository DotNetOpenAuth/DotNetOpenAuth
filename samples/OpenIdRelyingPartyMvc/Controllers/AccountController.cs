namespace RelyingPartyMvc.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Security;
	using System.Web.UI;

	public class AccountController : Controller {
		// This constructor is used by the MVC framework to instantiate the controller using
		// the default forms authentication and membership providers.

		public AccountController()
			: this(null, null) {
		}

		// This constructor is not used by the MVC framework but is instead provided for ease
		// of unit testing this type. See the comments at the end of this file for more
		// information.
		public AccountController(IFormsAuthentication formsAuth, IMembershipService service) {
			this.FormsAuth = formsAuth ?? new FormsAuthenticationService();
			this.MembershipService = service ?? new AccountMembershipService();
		}

		public IFormsAuthentication FormsAuth { get; private set; }

		public IMembershipService MembershipService { get; private set; }

		public ActionResult LogOn() {
			return View();
		}

		[AcceptVerbs(HttpVerbs.Post)]
		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
			Justification = "Needs to take same parameter type as Controller.Redirect()")]
		public ActionResult LogOn(string userName, bool rememberMe, string returnUrl) {
			this.FormsAuth.SignIn(userName, rememberMe);
			if (!String.IsNullOrEmpty(returnUrl)) {
				return Redirect(returnUrl);
			} else {
				return RedirectToAction("Index", "Home");
			}
		}

		public ActionResult LogOff() {
			this.FormsAuth.SignOut();

			return RedirectToAction("Index", "Home");
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext) {
			if (filterContext.HttpContext.User.Identity is WindowsIdentity) {
				throw new InvalidOperationException("Windows authentication is not supported.");
			}
		}
	}

	// The FormsAuthentication type is sealed and contains static members, so it is difficult to
	// unit test code that calls its members. The interface and helper class below demonstrate
	// how to create an abstract wrapper around such a type in order to make the AccountController
	// code unit testable.

	public interface IFormsAuthentication {
		void SignIn(string userName, bool createPersistentCookie);

		void SignOut();
	}

	public class FormsAuthenticationService : IFormsAuthentication {
		public void SignIn(string userName, bool createPersistentCookie) {
			FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
		}
		public void SignOut() {
			FormsAuthentication.SignOut();
		}
	}

	public interface IMembershipService {
		MembershipCreateStatus CreateUser(string claimedIdentifier, string email);
	}

	public class AccountMembershipService : IMembershipService {
		private MembershipProvider provider;
		private RandomNumberGenerator passwordGenerator;

		public AccountMembershipService()
			: this(null) {
		}

		public AccountMembershipService(MembershipProvider provider) {
			this.provider = provider ?? Membership.Provider;
			this.passwordGenerator = RNGCryptoServiceProvider.Create();
		}

		public MembershipCreateStatus CreateUser(string userName, string email) {
			MembershipCreateStatus status;
			string password = this.GenerateInsaneSecurePassword();
			this.provider.CreateUser(userName, password, email, null, null, true, null, out status);
			return status;
		}

		private string GenerateInsaneSecurePassword() {
			byte[] secureBits = new byte[20];
			this.passwordGenerator.GetBytes(secureBits);
			return Convert.ToBase64String(secureBits);
		}
	}
}
