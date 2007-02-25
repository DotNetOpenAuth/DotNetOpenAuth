using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Janrain.OpenId.Server
{
    public class CheckAuthRequest : AssociatedRequest
    {

        #region Private Members

        private string _sig;
        private NameValueCollection _signed;
        private string _invalidate_handle;

        #endregion

        #region Constructor(s)

        public CheckAuthRequest(string assoc_handle, string sig, NameValueCollection signed, string invalidate_handle)
        {
            this.AssocHandle = assoc_handle;
            _sig = sig;
            _signed = signed;
            _invalidate_handle = invalidate_handle;
        }

        public CheckAuthRequest(NameValueCollection query)
        {
            this.AssocHandle = this.GetField(query, "assoc_handle");
            _sig = this.GetField(query, "sig");
            string signedStr = this.GetField(query, "signed");

            _invalidate_handle = query.Get("openid.invalidate_handle");

            string[] signedList = signedStr.Split(',');
            string value = "";
            _signed = new NameValueCollection();


            foreach (string field in signedList)
            {
                if (field == "mode")
                    value = "id_res";
                else
                    value = this.GetSignedField(query, field);

                _signed.Add(field, value);
            }
        }

        #endregion

        #region Properties

        public override string Mode
        {
            get { return "check_authentication"; }
        }

        #endregion

        #region Methods

        public Response Answer(Signatory signatory)
        {
            bool is_valid = signatory.Verify(this.AssocHandle, _sig, _signed);

            signatory.Invalidate(this.AssocHandle, true);

            Response response = new Response(this);

            response.Fields["is_valid"] = (is_valid ? "true" : "false");

            if (_invalidate_handle != null && _invalidate_handle != "")
            {
                Association assoc = signatory.GetAssociation(_invalidate_handle, false);

                if (assoc == null)
                    response.Fields["invalidate_handle"] = _invalidate_handle;
            }

            return response;
        }

        private string GetSignedField(NameValueCollection query, string valueToFind)
        {
            string val = query.Get("openid." + valueToFind);

            if (val == null)
                throw new ProtocolException(query, "Couldn't find signed field " + valueToFind);

            return val;
        }

        private string GetField(NameValueCollection query, string valueToFind)
        {
            string val = query.Get("openid." + valueToFind);

            if (val == null)
                throw new ProtocolException(query, this.Mode + " request missing required parameter " + valueToFind);

            return val;
        }

        #endregion

    }
}
