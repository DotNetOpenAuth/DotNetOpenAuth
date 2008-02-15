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
        public EncodingException(IEncodable response)
        {
            Response = response;
        }

        public IEncodable Response { get; private set; }
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
    }

    /// <summary>
    /// Encodes responses in to <see cref="WebResponse"/>, signing them when required.
    /// </summary>
    internal class SigningEncoder : Encoder
    {
        Signatory signatory;

        public SigningEncoder(Signatory signatory)
        {
            this.signatory = signatory;
        }

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
                    if (signatory == null)
                        throw new ArgumentException("Must have a store to sign this request");

                    if (response.Fields.ContainsKey(QueryStringArgs.openidnp.sig))
                        throw new AlreadySignedException(encodable);

                    signatory.Sign(response);
                }

            }

            return base.Encode(encodable);
        }
    }
}
