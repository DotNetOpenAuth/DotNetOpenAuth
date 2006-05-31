namespace Janrain.OpenId.Server

import System
import System.Collections.Specialized
import Janrain.OpenId

class ProtocolException(ApplicationException, IEncodable):
    internal query as NameValueCollection

    HasReturnTo:
        get:
            return ('openid.return_to') in self.query

    WhichEncoding:
        get:
            if self.HasReturnTo:
                return EncodingType.ENCODE_URL

            mode = self.query.Get('openid.mode')
            if mode != null:
                if mode not in ('checkid_setup', 'checkid_immediate'):
                    return EncodingType.ENCODE_KVFORM

            # According to the OpenID spec as of this writing, we are
            # probably supposed to switch on request type here (GET
            # versus POST) to figure out if we're supposed to print
            # machine-readable or human-readable content at this
            # point.  GET/POST seems like a pretty lousy way of making
            # the distinction though, as it's just as possible that
            # the user agent could have mistakenly been directed to
            # post to the server URL.

            # Basically, if your request was so broken that you didn't
            # manage to include an openid.mode, I'm not going to worry
            # too much about returning you something you can't parse.
            return EncodingType.ENCODE_NONE


    def constructor(query as NameValueCollection, text as string):
        super(text)

        self.query = query

    def EncodeToUrl():
        return_to = self.query.Get('openid.return_to')
        if return_to is null:
            raise ApplicationException("I have no return_to URL.")

        q = NameValueCollection()
        q.Add('openid.mode', 'error')
        q.Add('openid.error', self.Message)

        builder = UriBuilder(return_to)
        UriUtil.AppendQueryArgs(builder, q)
        return Uri(builder.ToString(), true)

    def EncodeToKVForm():
        d = {'mode': 'error', 'error': self.Message,}
        return KVUtil.DictToKV(d)

