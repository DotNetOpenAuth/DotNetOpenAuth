namespace Janrain.OpenId.Server

import System
import System.Collections.Specialized


class EncodingException(ApplicationException):
    [Getter(Response)]
    response as IEncodable

    def constructor(response as IEncodable):
        self.response = response


class AlreadySignedException(EncodingException):
    def constructor(response as IEncodable):
        super(response)


class Encoder:
    virtual def Encode(response as IEncodable):
        encode_as = response.WhichEncoding
        if encode_as == EncodingType.ENCODE_KVFORM:
            if response isa Exception:
                code = HttpCode.HTTP_ERROR
            else:
                code = HttpCode.HTTP_OK

            wr = WebResponse(code, null, response.EncodeToKVForm())
        elif encode_as == EncodingType.ENCODE_URL:
            headers = NameValueCollection()
            headers.Add('Location', response.EncodeToUrl().AbsoluteUri)
            wr = WebResponse(HttpCode.HTTP_REDIRECT, headers, array(byte, 0))
        else:
            # Can't encode this to a protocol message.  You should probably
            # render it to HTML and show it to the user.
            raise EncodingException(response)

        return wr


class SigningEncoder(Encoder):
    internal signatory as Signatory

    def constructor(signatory as Signatory):
        self.signatory = signatory

    override def Encode(encodable as IEncodable):
        if not encodable isa Exception:
            response = cast(Response, encodable)
            if response.NeedsSigning:
                if self.signatory is null:
                    raise ArgumentException(
                        "Must have a store to sign this request")
                if 'sig' in response.Fields:
                    raise AlreadySignedException(response)

                self.signatory.Sign(response)

        return super.Encode(encodable)


class Decoder:
    static _handlers = {
        
        }

    static def Decode(query as NameValueCollection):
        if query is null:
            return null

        myquery = NameValueCollection()
        for key as string in query:
            if key.StartsWith("openid."):
                myquery[key] = query[key]

        if len(myquery) == 0:
            return null

        mode = myquery.Get('openid.mode')
        if mode is null:
            raise ProtocolException(
                query, "No openid.mode value in query")
        
        if mode == 'checkid_setup': 
            return CheckIdRequest(query)
        elif mode == 'checkid_immediate':
            return CheckIdRequest(query)
        elif mode == 'check_authentication':
            return CheckAuthRequest(query)
        elif mode == 'associate':
            return AssociateRequest(query)

        raise ProtocolException(
            query, "No decoder for openid.mode=${mode}")
