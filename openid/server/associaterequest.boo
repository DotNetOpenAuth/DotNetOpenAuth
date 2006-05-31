namespace Janrain.OpenId.Server

import System.Collections.Specialized
import Janrain.OpenId

class AssociateRequest(Request):
    override Mode as string:
        get:
            return "associate"

    internal assoc_type = "HMAC-SHA1"
    internal session as ServerSession
    
    def constructor(query as NameValueCollection):
        super()
        session_type = query.Get("openid.session_type")
        
        if session_type is null:
            self.session = PlainTextServerSession()
        elif session_type == "DH-SHA1":
            self.session = DiffieHellmanServerSession(query)
        else:
            raise ProtocolException(query,
                                    "Unknown session type ${session_type}")

    def Answer(assoc as Association):
        response = Response(self)
        response.Fields['expires_in'] = assoc.ExpiresIn
        response.Fields['assoc_type'] = 'HMAC-SHA1'
        response.Fields['assoc_handle'] = assoc.Handle

        nvc = self.session.Answer(assoc.Secret)
        for key as string in nvc:
            response.Fields[key] = nvc[key]

        if self.session.SessionType != 'plaintext':
            response.Fields['session_type'] = self.session.SessionType

        return response
