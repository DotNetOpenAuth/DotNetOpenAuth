using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace DotNetOpenId.Server
{
    public class MalformedReturnUrl : ProtocolException
    {

        #region Private Members

        private string _return_to;

        #endregion

        #region Constructor(s)

        public MalformedReturnUrl(NameValueCollection query, string return_to)
            : base(query, "")
        {
            _return_to = return_to;
        }

        #endregion

    }
}
