namespace Janrain.OpenId.Consumer

import System
import System.Net

class FetchResponse:
    public code as HttpStatusCode
    public finalUri as Uri
    public data as (byte)
    public length as int
    public charset as string

    def constructor(code as HttpStatusCode, finalUri as Uri,
		    charset as string, data as (byte), length as int):
        self.code = code
        self.finalUri = finalUri
        self.data = data
        self.length = length
        self.charset = charset

