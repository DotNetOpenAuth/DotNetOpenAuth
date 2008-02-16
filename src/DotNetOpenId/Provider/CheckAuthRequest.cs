using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;

namespace DotNetOpenId.Provider
{
    /// <summary>
    /// A request to verify the validity of a previous response.
    /// </summary>
    internal class CheckAuthRequest : AssociatedRequest
    {

        #region Private Members

        private string _sig;
        private IDictionary<string, string> _signed;
        private string _invalidate_handle;

        #endregion

        #region Constructor(s)

        public CheckAuthRequest(Server server, string assoc_handle, string sig,
            IDictionary<string, string> signed, string invalidate_handle) : base(server)
        {
            this.AssociationHandle = assoc_handle;
            _sig = sig;
            _signed = signed;
            _invalidate_handle = invalidate_handle;
        }

        public CheckAuthRequest(Server server, NameValueCollection query) : base(server)
        {
            this.AssociationHandle = this.GetField(query, QueryStringArgs.openidnp.assoc_handle);
            _sig = this.GetField(query, QueryStringArgs.openidnp.sig);
            string signedStr = this.GetField(query, QueryStringArgs.openidnp.signed);

            _invalidate_handle = query.Get(QueryStringArgs.openid.invalidate_handle);

            string[] signedList = signedStr.Split(',');
            string value = "";
            _signed = new Dictionary<string, string>();


            foreach (string field in signedList)
            {
                if (field == QueryStringArgs.openidnp.mode)
                    value = QueryStringArgs.Modes.id_res;
                else
                    value = this.GetSignedField(query, field);

                _signed.Add(field, value);
            }
        }

        #endregion

        #region Properties

        public override RequestType RequestType {
            get { return RequestType.CheckAuthRequest; }
        }

        /// <summary>
        /// Gets the string "check_authentication".
        /// </summary>
        internal override string Mode
        {
            get { return QueryStringArgs.Modes.check_authentication; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Respond to this request.
        /// </summary>
        internal IEncodable Answer()
        {
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("Start processing Response for CheckAuthRequest");
            }
            #endregion

            bool is_valid = Server.Signatory.Verify(this.AssociationHandle, _sig, _signed);

            Server.Signatory.Invalidate(this.AssociationHandle, true);

            Response response = new Response(this);

            response.Fields[QueryStringArgs.openidnp.is_valid] = (is_valid ? "true" : "false");

            if (_invalidate_handle != null && _invalidate_handle != "")
            {
                Association assoc = Server.Signatory.GetAssociation(_invalidate_handle, false);

                if (assoc == null)
                {
                    #region  Trace
                    if (TraceUtil.Switch.TraceInfo)
                    {
                        TraceUtil.ServerTrace("No matching association found. Returning invalidate_handle. ");
                    }
                    #endregion

                    response.Fields[QueryStringArgs.openidnp.invalidate_handle] = _invalidate_handle;
                }
            }

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("End processing Response for CheckAuthRequest. CheckAuthRequest response successfully created. ");
                if (TraceUtil.Switch.TraceVerbose)
                {
                    TraceUtil.ServerTrace("Response follows. ");
                    TraceUtil.ServerTrace(response.ToString());
                }
            }


            #endregion

            return response;
        }

        private string GetSignedField(NameValueCollection query, string valueToFind)
        {
            string val = query.Get(QueryStringArgs.openid.Prefix + valueToFind);

            if (val == null)
                throw new ProtocolException(query, "Couldn't find signed field " + valueToFind);

            return val;
        }

        private string GetField(NameValueCollection query, string valueToFind)
        {
            string val = query.Get(QueryStringArgs.openid.Prefix + valueToFind);

            if (val == null)
                throw new ProtocolException(query, this.Mode + " request missing required parameter " + valueToFind);

            return val;
        }

        #endregion


        public override string ToString()
        {
            string returnString = @"CheckAuthRequest._sig = '{0}'
CheckAuthRequest.AssocHandle = '{1}'
CheckAuthRequest._invalidate_handle = '{2}' ";
            return base.ToString() + Environment.NewLine + String.Format(returnString, _sig, AssociationHandle, _invalidate_handle);
        }

    }
}
