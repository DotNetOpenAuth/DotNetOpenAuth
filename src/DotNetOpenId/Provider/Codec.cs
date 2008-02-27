using System;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace DotNetOpenId.Provider
{
    /// <summary>
    /// Encodes responses in to <see cref="WebResponse"/>.
    /// </summary>
    internal class Encoder
    {
        /// <summary>
        /// Encodes responses in to WebResponses.
        /// </summary>
        public virtual Response Encode(IEncodable response)
        {
            EncodingType encode_as = response.EncodingType;
            Response wr;

            if (TraceUtil.Switch.TraceInfo)
            {
                Trace.TraceInformation("Encode using {0}", encode_as);
            }

            switch (encode_as)
            {
                case EncodingType.ResponseBody:
                    HttpStatusCode code = (response is Exception) ? 
                        HttpStatusCode.BadRequest : HttpStatusCode.OK;
                    wr = new Response(code, null, DictionarySerializer.Serialize(response.EncodedFields));
                    break;
                case EncodingType.RedirectBrowserUrl:
                    Debug.Assert(response.RedirectUrl != null);
                    NameValueCollection headers = new NameValueCollection();

                    UriBuilder builder = new UriBuilder(response.RedirectUrl);
                    UriUtil.AppendQueryArgs(builder, response.EncodedFields);
                    headers.Add("Location", builder.Uri.AbsoluteUri);

                    wr = new Response(HttpStatusCode.Redirect, headers, new byte[0]);
                    break;
                default:
                    if (TraceUtil.Switch.TraceError) {
                        Trace.TraceError("Cannot encode response: {0}", response);
                    }
                    wr = new Response(HttpStatusCode.BadRequest, null, new byte[0]);
                    break;
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

        public override Response Encode(IEncodable encodable)
        {
            var response = encodable as EncodableResponse;
            if (response != null)
            {
                if (TraceUtil.Switch.TraceInfo)
                {
                    Trace.TraceInformation("Encoding using the signing encoder");
                }
                
                if (response.NeedsSigning)
                {
                    if (signatory == null)
                        throw new ArgumentException("Must have a store to sign this request");

                    Debug.Assert(!response.Fields.ContainsKey(QueryStringArgs.openidnp.sig));

                    signatory.Sign(response);
                }

            }

            return base.Encode(encodable);
        }
    }
}
