using System;
using System.Collections.Specialized;
using System.Text;

namespace Janrain.OpenId.Server
{
    public class AssociateRequest : Request
    {

        #region Private Members

        private string _assoc_type = "HMAC-SHA1";
        private ServerSession _session;

        #endregion

        #region Constructor(s)

        public AssociateRequest(NameValueCollection query)
            : base()
        {
            string session_type = query.Get("openid.session_type");

            if (session_type == null)
            {
                _session = new PlainTextServerSession();
            }
            else if (session_type == "DH-SHA1")
            {
                _session = new DiffieHellmanServerSession(query);
            }
            else
            {
                throw new ProtocolException(query, "Unknown sessoin type " + session_type);
            }
        }

        #endregion

        #region Properties

        public override string Mode
        {
            get { return "associate"; }
        }

        #endregion

        #region Methods

        public Response Answer(Association assoc)
        {
            Response response = new Response(this);

            response.Fields["expires_in"] = assoc.ExpiresIn;
            response.Fields["assoc_type"] = "HMAC-SHA1";
            response.Fields["assoc_handle"] = assoc.Handle;

            NameValueCollection nvc = _session.Answer(assoc.Secret);
            foreach (string key in nvc)
            {
                response.Fields[key] = nvc[key];
            }

            if (_session.SessionType != "plaintext")
                response.Fields["session_type"] = _session.SessionType;

            return response;
        }

        #endregion

    }
}
