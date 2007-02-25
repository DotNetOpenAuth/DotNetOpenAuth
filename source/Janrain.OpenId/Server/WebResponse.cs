using System;
using System.Collections.Specialized;
using System.Text;

namespace Janrain.OpenId.Server
{

    public enum HttpCode : int
    {
        HTTP_OK = 200,
        HTTP_REDIRECT = 302,
        HTTP_ERROR = 400
    }

    public class WebResponse
    {

        #region Private Members

        private HttpCode _code;
        private NameValueCollection _headers;
        private byte[] _body;

        #endregion

        #region Constructor(s)

        public WebResponse(HttpCode code, NameValueCollection headers, byte[] body)
        {
            _code = code;

            if (headers == null)
                _headers = new NameValueCollection();
            else
                _headers = headers;

            _body = body;
        }

        #endregion

        #region Properties

        public HttpCode Code
        {
            get { return _code; }
        }

        public NameValueCollection Headers
        {
            get { return _headers; }
        }

        public byte[] Body
        {
            get { return _body; }
        }

        #endregion

    }
}
