namespace Janrain.Yadis

import System
import System.Collections.Specialized
import System.IO
import System.Net
import System.Text

class FetchResponse:
    [Getter(StatusCode)]
    status_code as HttpStatusCode

    [Getter(RequestUri)]
    request_uri as Uri

    [Getter(FinalUri)]
    final_uri as Uri

    [Getter(Body)]
    body as string

    [Getter(ContentType)]
    content_type as ContentType

    [Getter(Headers)]
    headers as NameValueCollection

    def constructor(request_uri as Uri, status_code as HttpStatusCode,
                    final_uri as Uri, content_type as ContentType,
                    body as string, headers as NameValueCollection):
        self.request_uri = request_uri
        self.status_code = status_code
        self.final_uri = final_uri
        self.content_type = content_type
        self.body = body
        self.headers = headers

class FetchRequest:
    static final DEFAULT_HTML_ENCODING = "ISO-8859-1"
    static final DEFAULT_XML_ENCODING = "utf-8"

    [Getter(Request)]
    request as HttpWebRequest

    url as Uri

    def constructor(url as Uri):
        self.url = url
        self.request = cast(HttpWebRequest, WebRequest.Create(url))
        self.request.KeepAlive = false
        self.request.Method = 'GET'
        self.request.MaximumAutomaticRedirections = 10
        
    
    protected static def ReadBytes(stream as Stream):
        buf = array(byte, 8192)
        body = array(byte, 0)

        while true:
            count = stream.Read(buf, 0, buf.Length)
            if count != 0:
                body += buf[:count]
                if len(body) > 1048576:
                    break
            else:
                break

        return body

    protected static def EncodingFromResp(resp as HttpWebResponse):
        content_type = resp.ContentType
        if (content_type == null):
            return null

        val = content_type.ToLower()
        pos = val.IndexOf("charset=")
        if (pos == -1):
            return null

        pos += 8;
        pos2 = val.IndexOf(';', pos)
        if (pos2 == -1):
            charset = content_type.Substring(pos)
        else:
            charset = content_type.Substring(pos, pos2 - pos).Trim()

        try:
            return Encoding.GetEncoding(charset)
        except why as ArgumentException:
            return null

    protected def MetaContentType(data as (byte), length as int,
                                  encoding as Encoding):
        if encoding is null:
            encoding = Encoding.GetEncoding(DEFAULT_HTML_ENCODING)

        html = encoding.GetString(data, 0, length)
        for attrs as NameValueCollection in ByteParser.HeadTagAttrs(html, "meta"):
            http_equiv as string = attrs.Get("http-equiv")
            if http_equiv != "content-type":
                continue

            content as string = attrs.Get("content")
            if content is null:
                continue

            return content
            return ContentType(content)

        return null

    def GetResponse(handle_html as bool):
        response = cast(HttpWebResponse, self.request.GetResponse())
        content_type as ContentType = null
        res_stream = response.GetResponseStream()
        body = ReadBytes(res_stream)
        header_enc = EncodingFromResp(response)
        enc_name = ByteParser.XmlEncoding(body, len(body), header_enc)
        if enc_name is null and handle_html:
            # Maybe HTML?
            default_enc_name = DEFAULT_HTML_ENCODING
            content_type = MetaContentType(body, len(body), header_enc)
            if content_type is not null:
                _enc_name = content_type.Parameters["charset"]
                if _enc_name is not null:
                    enc_name = _enc_name
        else:
            default_enc_name = DEFAULT_XML_ENCODING

        enc as Encoding
        try:
            enc = Encoding.GetEncoding(enc_name)
        except why as NotSupportedException:
            enc = Encoding.GetEncoding(default_enc_name)

        str = enc.GetString(body, 0, len(body))

        if content_type is null:
            content_type = ContentType(response.ContentType)

        return FetchResponse(self.url, response.StatusCode,
                             response.ResponseUri, content_type, str,
                             response.Headers)

