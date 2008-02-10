using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace DotNetOpenId.Server
{
    /// <summary>
    /// The trust root is not well-formed.
    /// </summary>
    public class MalformedTrustRootException : ProtocolException
    {

        #region Constructor(s)

        public MalformedTrustRootException(NameValueCollection query, string text)
            : base(query, text)
        {
        }

        #endregion

    }
}
