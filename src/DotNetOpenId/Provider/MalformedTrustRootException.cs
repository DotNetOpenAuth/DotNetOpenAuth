using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace DotNetOpenId.Provider
{
    /// <summary>
    /// The trust root is not well-formed.
    /// </summary>
    public class MalformedTrustRootException : ProtocolException
    {
        internal MalformedTrustRootException(NameValueCollection query, string text)
            : base(query, text)
        {
        }
    }
}
