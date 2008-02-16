using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Server
{

    // TODO Move this enum out to it's own file
    public enum EncodingType
    {
        None,
        UrlRedirection,
        KVForm
    }

    public interface IEncodable
    {
        EncodingType EncodingType { get; }
        IDictionary<string, string> EncodedFields { get; }
        Uri BaseUri { get; }
    }
}
