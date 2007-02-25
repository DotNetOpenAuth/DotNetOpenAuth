using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Janrain.OpenId.Server
{

    // TODO Move this out to it's own file
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

    // TODO Move this out to it's own file
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

    // TODO Move this out to it's own file
    public class MalformedTrustRoot : ProtocolException
    {

        #region Constructor(s)

        public MalformedTrustRoot(NameValueCollection query, string text)
            : base(query, text)
        {
        }

        #endregion

    }

    public class CheckIdRequest : AssociatedRequest
    {

        #region Private Members

        private bool _immediate;
        private string _trust_root;
        private Uri _identity;
        private string _mode;
        private Uri _return_to;

        #endregion

        #region Constructor(s)

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
            catch (ArgumentException)
            {
                throw new MalformedReturnUrl(null, _return_to.AbsoluteUri);
            }

            if (!this.TrustRootValid)
                throw new UntrustedReturnUrl(null, _return_to, _trust_root);

        }

        public CheckIdRequest(NameValueCollection query)
        {
            string mode = query["openid.mode"];

            if (mode == "checkid_immediate")
            {
                _immediate = true;
                _mode = "checkid_immediate";
            }
            else
            {
                _immediate = false;
                _mode = "checkid_setup";
            }

            string identity = GetField(query, "identity");
            try
            {
                _identity = new Uri(identity);
            }
            catch (UriFormatException)
            {
                throw new ProtocolException(query, "openid.identity not a valid url: " + identity);
            }

            string return_to = GetField(query, "return_to");
            try
            {
                _return_to = new Uri(return_to);
            }
            catch (UriFormatException)
            {
                throw new MalformedReturnUrl(query, return_to);
            }

            // TODO This just seems wonky to me
            _trust_root = query.Get("openid.trust_root");
            if (_trust_root == null)
                _trust_root = _return_to.AbsoluteUri;

            this.AssocHandle = query.Get("openid.assoc_handle");

            try
            {
                TrustRoot tr = new TrustRoot(_return_to.AbsoluteUri);
            }
            catch (ArgumentException)
            {
                throw new MalformedReturnUrl(query, _return_to.AbsoluteUri);
            }

            if (!TrustRootValid)
                throw new UntrustedReturnUrl(query, _return_to, _trust_root);

        }

        #endregion

        #region Private Methods

        private string GetField(NameValueCollection query, string field)
        {
            string value = query.Get("openid." + field);

            if (value == null)
                throw new ProtocolException(query, "Missing required field " + field);

            return value;
        }

        #endregion

        #region Public Methods

        public Response Answer(bool allow, Uri server_url)
        {
            string mode;

            if (allow || _immediate)
                mode = "id_res";
            else
                mode = "cancel";

            Response response = new Response(this);

            if (allow)
            {
                Hashtable fields = new Hashtable();

                fields.Add("mode", mode);
                fields.Add("identity", _identity.AbsoluteUri);
                fields.Add("return_to", _return_to.AbsoluteUri);

                response.AddFields(null, fields, true);

            }
            else
            {
                response.AddField(null, "mode", mode, false);
                if (_immediate)
                {
                    if (server_url == null)
                        throw new ApplicationException("setup_url is required for allow=False in immediate mode.");

                    CheckIdRequest setup_request = new CheckIdRequest(_identity, _return_to, _trust_root, false, this.AssocHandle);

                    Uri setup_url = setup_request.EncodeToUrl(server_url);

                    response.AddField(null, "user_setup_url", setup_url.AbsoluteUri, false);
                }
            }

            return response;
        }

        public Uri EncodeToUrl(Uri server_url)
        {
            NameValueCollection q = new NameValueCollection();

            q.Add("openid.mode", _mode);
            q.Add("openid.identity", _identity.AbsoluteUri);
            q.Add("openid.return_to", _return_to.AbsoluteUri);

            if (_trust_root != null)
                q.Add("openid.trust_root", _trust_root);

            if (this.AssocHandle != null)
                q.Add("openid.assoc_handle", this.AssocHandle);

            UriBuilder builder = new UriBuilder(server_url);
            Util.AppendQueryArgs(ref builder, q);

            return new Uri(builder.ToString());
        }

        public Uri GetCancelUrl()
        {
            if (_immediate)
                throw new ApplicationException("Cancel is not an appropriate response to immediate mode requests.");

            UriBuilder builder = new UriBuilder(_return_to);
            NameValueCollection args = new NameValueCollection();

            args.Add("openid.mode", "cancel");
            Util.AppendQueryArgs(ref builder, args);

            return new Uri(builder.ToString());
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

        #endregion

        #region Properties

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

        #endregion

        #region Inherited Properties

        public override string Mode
        {
            get { return _mode; }
        }

        #endregion

    }
}
