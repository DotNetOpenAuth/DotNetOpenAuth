namespace Janrain.Yadis

import System
import System.Collections.Specialized
import System.Net

class Yadis:
    public static final CONTENT_TYPE = 'application/xrds+xml'

    # A value suitable for using as an accept header when performing YADIS
    # discovery, unless the application has special requirements
    public static final ACCEPT_HEADER = AcceptHeader.Generate(
        ('text/html', 0.3),
        ('application/xhtml+xml', 0.5),
        (CONTENT_TYPE, 1.0))

    static final HEADER_NAME = 'X-XRDS-Location'

    static def MetaYadisLoc(html as string):
        for attrs as NameValueCollection in ByteParser.HeadTagAttrs(
            html, "meta"):
            http_equiv as string = attrs.Get("http-equiv")
            if http_equiv != HEADER_NAME.ToLower():
                continue

            value as string = attrs.Get("content")
            if value is null:
                continue

            try:
                return Uri(value)
            except why as UriFormatException:
                # XXX: log this
                continue

        return null

    static def Discover(uri as Uri):
        request = FetchRequest(uri)
        request.Request.Accept = Yadis.ACCEPT_HEADER
        init_resp = request.GetResponse(true)
        if init_resp.StatusCode != 200:
            #XXX: log this
            return null

        final_resp as FetchResponse
        # According to the spec, the content-type header must be an exact
        # match, or else we have to look for an indirection.
        if init_resp.ContentType.MediaType == Yadis.CONTENT_TYPE:
            final_resp = init_resp
        else:
            # Try the header
            yadis_loc_str = init_resp.Headers.Get(HEADER_NAME.ToLower())
            yadis_loc as Uri
            try:
                yadis_loc = Uri(yadis_loc_str)
            except why as UriFormatException:
                yadis_loc = null

            if (yadis_loc is null and
                init_resp.ContentType.MediaType == "text/html"):
                yadis_loc = MetaYadisLoc(init_resp.Body)

            if yadis_loc is not null:
                request = FetchRequest(yadis_loc)
                final_resp = request.GetResponse(false)
                if final_resp.StatusCode != 200:
                    #XXX: log this
                    return null

        return DiscoveryResult(uri, init_resp, final_resp)


class DiscoveryResult:
    [Getter(RequestUri)]
    request_uri as Uri

    [Getter(NormalizedUri)]
    normalized_uri as Uri
    
    [Getter(ContentType)]
    content_type as ContentType

    [Getter(YadisLocation)]
    yadis_location as Uri

    [Getter(ResponseText)]
    response_text as String

    UsedYadisLocation:
        get:
            return self.yadis_location is not null

    IsXRDS:
        get:
            return (self.UsedYadisLocation or
                    self.content_type.MediaType == Yadis.CONTENT_TYPE)


    def constructor(request_uri as Uri, init_resp as FetchResponse,
                    final_resp as FetchResponse):
        self.request_uri = request_uri
        self.normalized_uri = init_resp.FinalUri
        self.content_type = final_resp.ContentType
        self.response_text = final_resp.Body
        if init_resp is not final_resp:
            self.yadis_location = final_resp.RequestUri



