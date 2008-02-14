using System;
using System.Collections.Specialized;
using System.Text;

namespace DotNetOpenId.Server
{
    /// <summary>
    /// A request to establish an association.
    /// </summary>
    internal class AssociateRequest : Request
    {

        #region Private Members

        private string _assoc_type = QueryStringArgs.HMAC_SHA1;
        private ServerSession _session;

        #endregion

        #region Constructor(s)

        public AssociateRequest(NameValueCollection query)
            : base()
        {
            string session_type = query.Get(QueryStringArgs.openid.session_type);

            if (session_type == null)
            {
                _session = new PlainTextServerSession();
            }
            else if (session_type == QueryStringArgs.DH_SHA1)
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

        public override RequestMode RequestMode {
            get { return RequestMode.AssociateRequest; }
        }

        /// <summary>
        /// Returns the string "associate".
        /// </summary>
        internal override string Mode
        {
            get { return QueryStringArgs.Modes.associate; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Respond to this request with an association.
        /// </summary>
        public Response Answer(Association assoc)
        {

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("Start processing response for AssociateRequest");
                if (TraceUtil.Switch.TraceVerbose)
                {
                    TraceUtil.ServerTrace("Association to be sent follows:");
                    TraceUtil.ServerTrace(assoc.ToString());
                }
            }
            #endregion

            Response response = new Response(this);

            response.Fields[QueryStringArgs.openidnp.expires_in] = assoc.ExpiresIn.ToString();
            response.Fields[QueryStringArgs.openidnp.assoc_type] = QueryStringArgs.HMAC_SHA1;
            response.Fields[QueryStringArgs.openidnp.assoc_handle] = assoc.Handle;

            NameValueCollection nvc = _session.Answer(assoc.Secret);
            foreach (string key in nvc)
            {
                response.Fields[key] = nvc[key];
            }

            if (_session.SessionType != "plaintext")
            {
                response.Fields[QueryStringArgs.openidnp.session_type] = _session.SessionType;
            }

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("End processing response for AssociateRequest. AssociateRequest response successfully created. ");
                if (TraceUtil.Switch.TraceVerbose)
                {
                    TraceUtil.ServerTrace("Response follows. ");
                    TraceUtil.ServerTrace(response.ToString());
                }                
            }
            #endregion

            return response;
        }

        #endregion

        public override string ToString()
        {
            string returnString = "AssociateRequest._assoc_type = {0}";
            return base.ToString() + Environment.NewLine  + String.Format(returnString, _assoc_type);
        }

    }
}
