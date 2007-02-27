using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Janrain.OpenId.Server
{
    public class UntrustedReturnUrl : ProtocolException
    {

        #region Private Members

        private Uri _return_to;
        private string _trust_root;

        #endregion

        #region Constructor(s)

        public UntrustedReturnUrl(NameValueCollection query, Uri return_to, string trust_root)
            : base(query, "return_to " + return_to.AbsoluteUri + " not under trust_root " + trust_root)
        {
            _return_to = return_to;
            _trust_root = trust_root;
        }

        #endregion

    }
}
