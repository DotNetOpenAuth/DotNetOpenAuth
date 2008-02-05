using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace DotNetOpenId.Server
{
    /// <summary>
    /// The trust root is not well-formed.
    /// </summary>
    public class MalformedTrustRoot : ProtocolException
    {

        #region Constructor(s)

        public MalformedTrustRoot(NameValueCollection query, string text)
            : base(query, text)
        {
        }

        #endregion

    }
}
