using System;
using System.Collections.Specialized;
using System.Text;

namespace Janrain.OpenId.Server
{
    public class EncodingException : ApplicationException
    {

        #region Private Members

        private IEncodable _response;

        #endregion

        #region Constructor(s)

        public EncodingException(IEncodable response)
        {
            _response = response;
        }

        #endregion

        #region Properties

        public IEncodable Response
        {
            get { return _response; }
        }

        #endregion

    }

    public class AlreadySignedException : EncodingException
    {

        public AlreadySignedException(IEncodable response)
            : base(response)
        {
        }
        
    }

    public class Encoder
    {

        #region Constructor(s)

        public Encoder() { }

        #endregion

        #region Methods

        public virtual WebResponse Encode(IEncodable response)
        {
            EncodingType encode_as = response.WhichEncoding;
            WebResponse wr;


            if (encode_as == EncodingType.ENCODE_KVFORM)
            {
                HttpCode code;

                if (response is Exception)
                    code = HttpCode.HTTP_ERROR;
                else
                    code = HttpCode.HTTP_OK;

                wr = new WebResponse(code, null, response.EncodeToKVForm());

            }
            else if (encode_as == EncodingType.ENCODE_URL)
            {
                NameValueCollection headers = new NameValueCollection();

                headers.Add("Location", response.EncodeToUrl().AbsoluteUri);

                wr = new WebResponse(HttpCode.HTTP_REDIRECT, headers, new byte[0]);
            }
            else
            {
                throw new EncodingException(response);
            }

            return wr;
        }

        #endregion

    }

    public class SigningEncoder : Encoder
    {

        #region Private Members

        private Signatory _signatory;

        #endregion

        #region Constructor(s)

        public SigningEncoder(Signatory signatory)
        {
            _signatory = signatory;
        }

        #endregion

        #region Methods

        public override WebResponse Encode(IEncodable encodable)
        {
            if (!(encodable is Exception))
            {
                Response response = (Response)encodable;

                if (response.NeedsSigning)
                {
                    if (_signatory == null)
                        throw new ArgumentException("Must have a store to sign this request");

                    if (response.Fields.Contains("sig"))
                        throw new AlreadySignedException(encodable);

                    _signatory.Sign(response);
                }

            }

            return base.Encode(encodable);
        }

        #endregion

    }

    public class Decoder
    {

        #region Private Members

        private static string[] _handlers = { };

        #endregion

        #region Methods

        public static Request Decode(NameValueCollection query)
        {
            if (query == null) return null;

            NameValueCollection myquery = new NameValueCollection();
            foreach (string key in query)
            {
                if (key.StartsWith("openid."))
                    myquery[key] = query[key];
            }

            if (myquery.Count == 0) return null;

            string mode = myquery.Get("openid.mode");
            if (mode == null)
                throw new ProtocolException(query, "No openid.mode value in query");

            if (mode == "checkid_setup")
                return new CheckIdRequest(query);
            else if (mode == "checkid_immediate")
                return new CheckIdRequest(query);
            else if (mode == "check_authentication")
                return new CheckAuthRequest(query);
            else if (mode == "associate")
                return new AssociateRequest(query);

            throw new ProtocolException(query, "No decoder for openid.mode=" + mode);

        }

        #endregion

    }


}
