using System;
using System.Collections.Specialized;
using System.Text;
using System.Net;

namespace DotNetOpenId.Server
{
    /// <summary>
    /// Could not encode this as a protocol message.
    /// </summary>
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

    /// <summary>
    /// This response is already signed.
    /// </summary>
    public class AlreadySignedException : EncodingException
    {

        public AlreadySignedException(IEncodable response)
            : base(response)
        {
        }

    }

    /// <summary>
    /// Encodes responses in to <see cref="WebResponse"/>.
    /// </summary>
    internal class Encoder
    {

        #region Constructor(s)

        public Encoder() { }

        #endregion

        #region Methods
        /// <summary>
        /// Encodes responses in to WebResponses.
        /// </summary>
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
                HttpStatusCode code;

                if (response is Exception)
                    code = HttpStatusCode.BadRequest;
                else
                    code = HttpStatusCode.OK;

                wr = new WebResponse(code, null, response.EncodeToKVForm());
            }
            else if (encode_as == EncodingType.ENCODE_URL)
            {
                NameValueCollection headers = new NameValueCollection();

                headers.Add("Location", response.EncodeToUrl().AbsoluteUri);

                wr = new WebResponse(HttpStatusCode.Redirect, headers, new byte[0]);
            }
            else
            {
                throw new EncodingException(response);
            }

            return wr;
        }

        #endregion

    }

    /// <summary>
    /// Encodes responses in to <see cref="WebResponse"/>, signing them when required.
    /// </summary>
    internal class SigningEncoder : Encoder
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

                    if (response.Fields.ContainsKey(QueryStringArgs.openidnp.sig))
                        throw new AlreadySignedException(encodable);

                    _signatory.Sign(response);
                }

            }

            return base.Encode(encodable);
        }

        #endregion

    }
}
