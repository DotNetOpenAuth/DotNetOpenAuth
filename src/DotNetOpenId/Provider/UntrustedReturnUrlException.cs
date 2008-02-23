using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// A return_to is outside the trust_root.
	/// </summary>
	internal class UntrustedReturnUrlException : ProtocolException {
		internal UntrustedReturnUrlException(Uri returnTo, string trustRoot, NameValueCollection query)
			: base(string.Format(Strings.ReturnToNotUnderTrustRoot, returnTo.AbsoluteUri, trustRoot), query) {
		}
	}
}