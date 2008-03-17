using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using DotNetOpenId.Yadis;

namespace Janrain.Yadis
{
    class FetchRequest
    {
        protected const string DEFAULT_HTML_ENCODING = "ISO-8859-1";
        protected const string DEFAULT_XML_ENCODING = "utf-8";
        protected HttpWebRequest request;
        protected Uri url;

        public FetchRequest(Uri url)
        {
            this.url = url;
            this.request = (HttpWebRequest)WebRequest.Create(url);
            this.request.KeepAlive = false;
            this.request.Method = "GET";
            this.request.MaximumAutomaticRedirections = 10;
        }

        protected static Encoding EncodingFromResp(HttpWebResponse resp)
        {
            string contentType = resp.ContentType;
            if (contentType == null)
            {
                return null;
            }
            string val = contentType.ToLower();
            int startIndex = val.IndexOf("charset=");
            if (startIndex == -1)
            {
                return null;
            }
            startIndex += 8;
            int index = val.IndexOf(";", startIndex);
            string name;
            if (index == -1)
            {
                name = contentType.Substring(startIndex);
            }
            else
            {
                name = contentType.Substring(startIndex, index - startIndex).Trim();
            }
            try
            {
                return Encoding.GetEncoding(name);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public FetchResponse GetResponse(bool handle_html)
        {
            string defaultEncName;
            HttpWebResponse resp = (HttpWebResponse)this.request.GetResponse();
            ContentType type = null;
            Stream responseStream = resp.GetResponseStream();

            Encoding encoding = EncodingFromResp(resp);

            //TODO: I totally factored out ReadBytes, but this whole response section needs
            // to be more .NET-y. The framework does a LOT of this stuff for us for free...
            string data = String.Empty;
			using (StreamReader b = new StreamReader(responseStream, encoding ?? Encoding.GetEncoding("ISO-8859-1")))
            {
                data = b.ReadToEnd();
            }

            string foundEndName = ByteParser.XmlEncoding(data, data.Length, encoding);
            if ((foundEndName == null) && handle_html)
            {
                //Maybe HTML?
                defaultEncName = "ISO-8859-1";
                type = this.MetaContentType(data, data.Length, encoding);
                if (type != null)
                {
                    string encName = type.Parameters["charset"];
                    if (encName != null)
                    {
                        foundEndName = encName;
                    }
                }
            }
            else
            {
                defaultEncName = "utf-8";
            }
            Encoding enc;
            if (foundEndName == null)
                enc = Encoding.GetEncoding(defaultEncName);
            else
            {
                try
                {
                    enc = Encoding.GetEncoding(foundEndName);
                }
                catch (NotSupportedException)
                {
                    enc = Encoding.GetEncoding(defaultEncName);
                }
            }

            //TODO: We've already got the body in the string "data"...changed the last line also
            //string body = enc.GetString(data, 0, data.Length);
            if (type == null)
            {
                type = new ContentType(resp.ContentType);
            }
            return new FetchResponse(this.url, resp.StatusCode, resp.ResponseUri, type, data, resp.Headers);
        }

        protected ContentType MetaContentType(string html, int length, Encoding encoding)
        {
            object[] attrsArray = ByteParser.HeadTagAttrs(html, "meta");
            foreach (NameValueCollection values in attrsArray)
            {
                string http_equiv = values["http-equiv"];
                if ((http_equiv != null) && (http_equiv.ToLower() == "content-type"))
                {
                    string content = values.Get("content");
                    if (content != null)
                    {
                        return new ContentType(content);
                    }
                }
            }
            return null;
        }
    }
}
