namespace Janrain.OpenId.Server

import System

enum EncodingType:
    ENCODE_NONE
    ENCODE_URL
    ENCODE_KVFORM

interface IEncodable:
    WhichEncoding as EncodingType:
        get:
            pass

    def EncodeToUrl() as Uri

    def EncodeToKVForm() as (byte)

