using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Provider
{

    // TODO Move this enum out to it's own file
    internal enum EncodingType
    {
        None,
        UrlRedirection,
        KVForm
    }

    internal interface IEncodable
    {
        EncodingType EncodingType { get; }
        IDictionary<string, string> EncodedFields { get; }
        Uri BaseUri { get; }
    }
}
