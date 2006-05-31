namespace Janrain.OpenId.Server

import System.Collections.Specialized

enum HttpCode:
    HTTP_OK = 200
    HTTP_REDIRECT = 302
    HTTP_ERROR = 400


class WebResponse:
    [Getter(Code)]
    code as HttpCode
    
    [Getter(Headers)]
    headers as NameValueCollection

    [Getter(Body)]
    body as (byte)
    
    def constructor(code as HttpCode, headers as NameValueCollection, 
                    body as (byte)):
        self.code = code

        if headers is null:
            self.headers = NameValueCollection()
        else:
            self.headers = headers

        self.body = body

