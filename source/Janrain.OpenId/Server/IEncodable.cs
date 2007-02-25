using System;
using System.Collections.Generic;
using System.Text;

namespace Janrain.OpenId.Server
{

    // TODO Move this enum out to it's own file
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
