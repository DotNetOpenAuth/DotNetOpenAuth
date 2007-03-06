using System;
using System.Collections.Generic;
using System.Text;
using Janrain.OpenId;
using System.Collections.Specialized;

namespace Janrain.OpenId.Consumer
{
    public class AuthRequest
    {
        public enum Mode
        {
            IMMEDIATE,
            SETUP
        }

        private string _token;
        public string Token
        {
            get
            {
                return _token;
            }
            set
            {
                _token = value;
            }
        }

        private NameValueCollection _extraArgs;
        public NameValueCollection ExtraArgs
        {
            get
            {
                return _extraArgs;
            }
        }

        private NameValueCollection _returnToArgs;
        public NameValueCollection ReturnToArgs
        {
            get
            {
                return ReturnToArgs;
            }
        }

        private Association _assoc;
        private ServiceEndpoint _endpoint;

        public AuthRequest(string token, Association assoc, ServiceEndpoint endpoint)
        {
            _token = token;
            _assoc = assoc;
            _endpoint = endpoint;

            _extraArgs = new NameValueCollection();
            _returnToArgs = new NameValueCollection();
        }

        public Uri CreateRedirect(string trustRoot, Uri returnTo, Mode mode)
        {
            string modeStr = String.Empty;
            if (mode == Mode.IMMEDIATE)
                modeStr = "checkid_immediate";
            else if (mode == Mode.SETUP)
                modeStr = "checkid_setup";

            UriBuilder returnToBuilder = new UriBuilder(returnTo);
            UriUtil.AppendQueryArgs(ref returnToBuilder, this.ReturnToArgs);

            NameValueCollection qsArgs = new NameValueCollection();
            qsArgs.Add("openid.mode", modeStr);
            qsArgs.Add("openid.identity", this._endpoint.ServerId.AbsoluteUri); //TODO: breaks the Law of Demeter
            qsArgs.Add("openid.return_to", new Uri(returnToBuilder.ToString(), true).AbsoluteUri); //TODO: obsolete, problem?
            qsArgs.Add("openid.trust_root", trustRoot);

            if (this._assoc != null)
                qsArgs.Add("openid.assoc", this._assoc.Handle);

            UriBuilder redir = new UriBuilder(this._endpoint.ServerUrl);
            UriUtil.AppendQueryArgs(ref redir, qsArgs);
            UriUtil.AppendQueryArgs(ref redir, this.ExtraArgs);
            return new Uri(redir.ToString(), true);
        }

    }
}

