using System;
using System.Collections.Generic;
using System.Text;

namespace Janrain.OpenId.Server
{

    public enum EncodingType
    {
        ENCODE_NONE,
        ENCODE_URL,
        ENCODE_KVFORM
    }

    public interface IEncodable
    {
        EncodingType WhichEncoding { get; }

        Uri EncodeToUrl();
        byte[] EncodeToKVForm();
    }
}
