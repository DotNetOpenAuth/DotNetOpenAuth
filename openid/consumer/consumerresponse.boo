namespace Janrain.OpenId.Consumer

import System
import System.Collections


class ConsumerResponse:
    [Getter(IdentityUrl)]
    identity_url as Uri

    signed_args as IDictionary

    ReturnTo:
        get:
            return Uri(self.signed_args['openid.return_to'], true)

    def constructor(identity_url as Uri, query as IDictionary,
                    signed as string):
        self.identity_url = identity_url
        self.signed_args = {}
        for field_name in /,/.Split(signed):
            field_name = 'openid.' + field_name
            val = query[field_name]
            if val is null:
                val = String.Empty
            self.signed_args[field_name] = val

    def ExtensionResponse(prefix as string) as IDictionary:
        response = {}
        prefix = 'openid.${prefix}.'
        prefix_len = len(prefix)
        for pair as DictionaryEntry in self.signed_args:
            k = cast(string, pair.Key)
            if k.StartsWith(prefix):
                response_key = k[prefix_len:]
                response[response_key] = pair.Value

        return response


