using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using DotNetOpenId.Yadis;

namespace Janrain.Yadis
{
    [Serializable]
    internal class FetchResponse
    {
        protected string body;
        protected ContentType contentType;
        protected Uri finalUri;
        protected NameValueCollection headers;
        protected Uri requestUri;
        protected HttpStatusCode statusCode;

        public FetchResponse(Uri requestUri, HttpStatusCode statusCode, Uri finalUri, ContentType contentType, string body, NameValueCollection headers)
        {
            this.requestUri = requestUri;
            this.statusCode = statusCode;
            this.finalUri = finalUri;
            this.contentType = contentType;
            this.body = body;
            this.headers = headers;
        }

        public string Body
        {
            get
            {
                return this.body;
            }
        }

        public ContentType ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        public Uri FinalUri
        {
            get
            {
                return this.finalUri;
            }
        }

        public NameValueCollection Headers
        {
            get
            {
                return this.headers;
            }
        }

        public Uri RequestUri
        {
            get
            {
                return this.requestUri;
            }
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                return this.statusCode;
            }
        }
    }
}
