namespace MvcRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using DotNetOpenAuth.OpenId;

	/// <summary>
	/// Forms authentication interface to facilitate login/logout functionality.
	/// </summary>
	/// <remarks>
	/// The FormsAuthentication type is sealed and contains static members, so it is difficult to
	/// unit test code that calls its members. The interface and helper class below demonstrate
	/// how to create an abstract wrapper around such a type in order to make the AccountController
	/// code unit testable.
	/// </remarks>
	public interface IFormsAuthentication {
		void SignIn(Identifier claimedIdentifier, bool createPersistentCookie);

		void SignOut();
	}

	/// <summary>
	/// The standard FormsAuthentication behavior to use for the live site.
	/// </summary>
	public class FormsAuthenticationService : IFormsAuthentication {
		public void SignIn(Identifier claimedIdentifier, bool createPersistentCookie) {
			FormsAuthentication.SetAuthCookie(claimedIdentifier, createPersistentCookie);
		}

		public void SignOut() {
			FormsAuthentication.SignOut();
		}
	}
}
