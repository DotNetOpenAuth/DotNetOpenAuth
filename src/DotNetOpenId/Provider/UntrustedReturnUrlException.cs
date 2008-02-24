using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// A return_to is outside the trust_root.
	/// </summary>
	[Serializable]
	internal class UntrustedReturnUrlException : ProtocolException {
		internal UntrustedReturnUrlException(Uri returnTo, string trustRoot, NameValueCollection query)
			: base(string.Format(CultureInfo.CurrentUICulture, Strings.ReturnToNotUnderTrustRoot, returnTo.AbsoluteUri, trustRoot), query) {
		}
	}
}