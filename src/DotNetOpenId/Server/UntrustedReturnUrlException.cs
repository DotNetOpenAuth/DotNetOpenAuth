using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace DotNetOpenId.Server
{
    /// <summary>
    /// A return_to is outside the trust_root.
    /// </summary>
    public class UntrustedReturnUrlException : ProtocolException
    {
        Uri _return_to;
        string _trust_root;

        internal UntrustedReturnUrlException(NameValueCollection query, Uri return_to, string trust_root)
            : base(query, "return_to " + return_to.AbsoluteUri + " not under trust_root " + trust_root)
        {
            _return_to = return_to;
            _trust_root = trust_root;
        }
    }
}
