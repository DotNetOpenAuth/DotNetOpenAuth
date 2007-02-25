using System;
using System.Collections.Specialized;
using System.Text;

namespace Janrain.OpenId.Server
{
    public class UntrustedReturnUrl : ProtocolException
    {
        private Uri _return_to;
        private string _trust_root;

        public UntrustedReturnUrl(NameValueCollection query, Uri return_to, string trust_root)
            : base(query, "return_to " + return_to.AbsoluteUri + " not under trust_root " + trust_root)
        {
            _return_to = return_to;
            _trust_root = trust_root;
        }
    }

    public class MalformedReturnUrl : ProtocolException
    {
        private string _return_to;

        public MalformedReturnUrl(NameValueCollection query, string return_to)
            : base(query, "")
        {
            _return_to = return_to;
        }
    }

    public class MalformedTrustRoot : ProtocolException
    {
        public MalformedTrustRoot(NameValueCollection query, string text)
            : base(query, text)
        {
        }
    }

    public class CheckIdRequest : AssociatedRequest
    {
        private bool _immediate;
        private string _trust_root;
        private Uri _identity;
        private string _mode;
        private Uri _return_to;

        public CheckIdRequest(Uri identity, Uri return_to, string trust_root, bool immediate, string assoc_handle)
        {
            this.AssocHandle = assoc_handle;

            _identity = identity;
            _return_to = return_to;

            if (trust_root == null)
                _trust_root = return_to.AbsoluteUri;
            else
                _trust_root = trust_root;

            _immediate = immediate;
            if (_immediate)
                _mode = "checkid_immediate";
            else
                _mode = "checkid_setup";

            try
            {
                TrustRoot trustRoot = new TrustRoot(_return_to.AbsolutePath);
            }
            catch (ArgumentException e)
            {
                throw new MalformedReturnUrl(null, _return_to.AbsoluteUri);
            }

            if (!this.TrustRootValid)
                throw new UntrustedReturnUrl(null, _return_to, _trust_root);

        }

        public bool TrustRootValid
        {
            get
            {
                // TODO this doesn't seem right to me
                if (_trust_root == null)
                    return true;

                TrustRoot tr = new TrustRoot(_trust_root);
                if (tr == null)
                    throw new MalformedTrustRoot(null, _trust_root);

                return tr.ValidateUrl(_return_to);
            }
        }

        public bool Immediate
        {
            get { return _immediate; }
        }

        public string TrustRoot
        {
            get { return _trust_root; }
        }

        public Uri IdentityUrl
        {
            get { return _identity; }
        }

        public Uri ReturnTo
        {
            get { return _return_to; }
        }

        public override string Mode
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
    }
}
