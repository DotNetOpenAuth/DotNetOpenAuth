using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace DotNetOpenId.Server
{
    public class MalformedReturnUrlException : ProtocolException
    {

        #region Private Members

        private string _return_to;

        #endregion

        #region Constructor(s)

        public MalformedReturnUrlException(NameValueCollection query, string return_to)
            : base(query, "")
        {
            _return_to = return_to;
        }

        #endregion

    }
}
