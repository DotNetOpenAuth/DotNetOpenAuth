using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Consumer {
	/// <summary>
	/// The authentication request was canceled on the OpenID provider's web site.
	/// </summary>
	public class CancelException : OpenIdException {
		public CancelException(Uri identityUrl)
			: base(string.Empty, identityUrl) {
		}
	}

}
