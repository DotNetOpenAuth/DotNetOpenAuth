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

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace(String.Format("Encode using {0}", encode_as));
            }
            #endregion
            
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
                #region  Trace
                if (TraceUtil.Switch.TraceInfo)
                {
                    TraceUtil.ServerTrace("Encoding using the signing encoder");
                }
                #endregion                          
                
                
                Response response = (Response)encodable;

                if (response.NeedsSigning)
                {
                    if (_signatory == null)
                        throw new ArgumentException("Must have a store to sign this request");

                    if (response.Fields.Contains(QueryStringArgs.openidnp.sig))
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
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("Start message decoding");                
            }
            #endregion                            
            
            if (query == null) return null;

            NameValueCollection myquery = new NameValueCollection();
            foreach (string key in query)
            {
                if (!String.IsNullOrEmpty(key))
                {
                    if (key.StartsWith(QueryStringArgs.openid.Prefix)) { myquery[key] = query[key]; }
                }
            }

            if (myquery.Count == 0) return null;

            string mode = myquery.Get(QueryStringArgs.openid.mode);
            if (mode == null)
                throw new ProtocolException(query, "No openid.mode value in query");

            if (mode == QueryStringArgs.Modes.checkid_setup)
            {
                CheckIdRequest request = new CheckIdRequest(query);
                
                #region  Trace 
                if (TraceUtil.Switch.TraceInfo)
                {
                    TraceUtil.ServerTrace("End message decoding. Successfully decoded message as new CheckIdRequest in setup mode");
                    if (TraceUtil.Switch.TraceInfo)
                    {
                        TraceUtil.ServerTrace("CheckIdRequest follows: ");
                        TraceUtil.ServerTrace(request.ToString());
                    }
                }
                #endregion                
                
                return request;
            }
            else if (mode == QueryStringArgs.Modes.checkid_immediate)
            {
                CheckIdRequest request = new CheckIdRequest(query);

                #region  Trace
                if (TraceUtil.Switch.TraceInfo)
                {
                    TraceUtil.ServerTrace("End message decoding. Successfully decoded message as new CheckIdRequest in immediate mode");
                    if (TraceUtil.Switch.TraceInfo)
                    {
                        TraceUtil.ServerTrace("CheckIdRequest follows: ");
                        TraceUtil.ServerTrace(request.ToString());
                    }
                }
                #endregion                   
                
                return request;
            }
            else if (mode == QueryStringArgs.Modes.check_authentication)
            {
                CheckAuthRequest request = new CheckAuthRequest(query);

                #region  Trace
                if (TraceUtil.Switch.TraceInfo)
                {
                    TraceUtil.ServerTrace("End message decoding. Successfully decoded message as new CheckAuthRequest");
                    if (TraceUtil.Switch.TraceInfo)
                    {
                        TraceUtil.ServerTrace("CheckAuthRequest follows: ");
                        TraceUtil.ServerTrace(request.ToString());
                    }
                }
                #endregion          
                
                return request;
            }
            else if (mode == QueryStringArgs.Modes.associate)
            {
                AssociateRequest request = new AssociateRequest(query);

                #region  Trace
                if (TraceUtil.Switch.TraceInfo)
                {
                    TraceUtil.ServerTrace("End message decoding. Successfully decoded message as new AssociateRequest ");
                    if (TraceUtil.Switch.TraceInfo)
                    {
                        TraceUtil.ServerTrace("AssociateRequest follows: ");
                        TraceUtil.ServerTrace(request.ToString());
                    }
                }
                #endregion                         
                
                return request;
            }

            throw new ProtocolException(query, "No decoder for openid.mode=" + mode);

        }

        #endregion

    }


}
