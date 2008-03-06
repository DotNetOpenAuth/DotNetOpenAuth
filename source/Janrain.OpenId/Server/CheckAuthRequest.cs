using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;

namespace Janrain.OpenId.Server
{
    public class CheckAuthRequest : AssociatedRequest
    {

        #region Private Members

        private string _sig;
        private NameValueCollection _signed;
        private IList<string> _signed_order;
        private string _invalidate_handle;

        #endregion

        #region Constructor(s)

        public CheckAuthRequest(string assoc_handle, string sig, NameValueCollection signed, IList<string> signed_order, string invalidate_handle)
        {
            this.AssocHandle = assoc_handle;
            _sig = sig;
            _signed = signed;
            _signed_order = signed_order;
            _invalidate_handle = invalidate_handle;
        }

        public CheckAuthRequest(NameValueCollection query)
        {
            this.AssocHandle = this.GetField(query, QueryStringArgs.openidnp.assoc_handle);
            _sig = this.GetField(query, QueryStringArgs.openidnp.sig);
            string signedStr = this.GetField(query, QueryStringArgs.openidnp.signed);

            _invalidate_handle = query.Get(QueryStringArgs.openid.invalidate_handle);

            string[] signedList = signedStr.Split(',');
            string value = "";
            _signed = new NameValueCollection();


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

        public override string Mode
        {
            get { return QueryStringArgs.Modes.check_authentication; }
        }

        #endregion

        #region Methods

        public Response Answer(Signatory signatory)
        {
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("Start processing Response for CheckAuthRequest");
            }
            #endregion

			bool is_valid = signatory.Verify(this.AssocHandle, _sig, _signed, _signed_order);

            signatory.Invalidate(this.AssocHandle, true);

            Response response = new Response(this);

            response.Fields[QueryStringArgs.openidnp.is_valid] = (is_valid ? "true" : "false");

            if (_invalidate_handle != null && _invalidate_handle != "")
            {
                Association assoc = signatory.GetAssociation(_invalidate_handle, false);

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
            return base.ToString() + Environment.NewLine + String.Format(returnString, _sig, AssocHandle, _invalidate_handle);
        }

    }
}
