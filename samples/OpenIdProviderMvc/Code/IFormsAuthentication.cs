namespace OpenIdProviderMvc.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
using System.Web.Security;

	/// <summary>
	/// An interface that wraps the <see cref="FormsAuthentication"/> type.
	/// </summary>
	/// <remarks>
	/// The FormsAuthentication type is sealed and contains static members, so it is difficult to
	/// unit test code that calls its members. The interface and helper class below demonstrate
	/// how to create an abstract wrapper around such a type in order to make the AccountController
	/// code unit testable.
	/// </remarks>
	public interface IFormsAuthentication {
		string SignedInUsername { get; }

		DateTime? SignedInTimestampUtc { get; }

		void SignIn(string userName, bool createPersistentCookie);

		void SignOut();
	}
}
