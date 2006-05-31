namespace Janrain.OpenId.Server

import System
import System.Collections
import System.Collections.Specialized
import Janrain.OpenId

class Response(IEncodable):
    [Getter(Request)]
    request as Request
    
    internal fields as Hash
    internal signed as List

    Fields as IDictionary:
        get:
            return self.fields

    Signed as (string):
        get:
            return array(string, self.signed)
    
    NeedsSigning:
        get:
            return (
                (self.request.Mode in ['checkid_setup', 'checkid_immediate'])
                and (len(self.signed) > 0))
    
    WhichEncoding:
        get:
            if self.request.Mode in ('checkid_setup', 'checkid_immediate'):
                return EncodingType.ENCODE_URL
            else:
                return EncodingType.ENCODE_KVFORM
    
    
    def constructor(request as Request):
        self.request = request
        self.fields = {}
        self.signed = []
    
    def AddField(nmspace as string, key as string, value as string,
                 signed as bool):
        if nmspace is not null and nmspace != String.Empty:
            key = "${nmspace}.${key}"
        self.fields[key] = value
        if signed and key not in self.signed:
            self.signed.Add(key)
    
    def AddFields(nmspace as string, fields as IDictionary,
                  signed as bool):
        for pair as DictionaryEntry in fields:
            self.AddField(nmspace, cast(string, pair.Key),
                          cast(string, pair.Value), signed)

    def Update(nmspace as string, other as Response):
        nmspaced_fields as Hash
        nmspaced_signed as List
        if nmspace is null or nmspace == String.Empty:
            nmspaced_fields = other.fields
            nmspaced_signed = other.signed
        else:
            nmspaced_fields = Hash(("${nmspace}.${pair.Key}", pair.Value)
                                     for pair in other.fields)
            nmspaced_signed = ["${nmspace}.${k}" for k in other.signed]

        for pair in nmspaced_fields:
            self.fields.Add(pair.Key, pair.Value)

        self.signed += nmspaced_signed

    def EncodeToUrl():
        nvc = NameValueCollection()
        for pair as DictionaryEntry in self.fields:
            nvc.Add("openid." + cast(string, pair.Key),
                    cast(string, pair.Value))
        
        checkidreq = cast(CheckIdRequest, self.request)
        builder = UriBuilder(checkidreq.ReturnTo)
        UriUtil.AppendQueryArgs(builder, nvc)
        return Uri(builder.ToString(), true)

    def EncodeToKVForm():
        return KVUtil.DictToKV(self.fields)
