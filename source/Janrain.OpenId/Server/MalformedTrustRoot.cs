using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Janrain.OpenId.Server
{
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
