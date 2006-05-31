namespace Janrain.OpenId

import System
import System.Collections
import System.Collections.Specialized
import System.Text
import System.Text.RegularExpressions
import System.Web

class UriUtil:
    private def constructor():
        pass

    static def NormalizeUri(uriStr as string):
        if ((not uriStr.StartsWith('http')) and
            (uriStr.IndexOf('://') == (-1))):
            uriStr = ('http://' + uriStr)
        bldr = UriBuilder(uriStr)
        bldr.Host = bldr.Host.ToLower()
        return bldr.Uri

    static def CreateQueryString(args as NameValueCollection):
        parts = array(string, args.Count)
        for i in range(args.Count):
            encKey = HttpUtility.UrlEncode(args.GetKey(i))
            encVal = HttpUtility.UrlEncode(args.Get(i))
            parts[i] = "${encKey}=${encVal}"

        return String.Join('&', parts)

    static def AppendQueryArg(builder as UriBuilder, key as string,
                              value as string):
        encKey = HttpUtility.UrlEncode(key)
        encVal = HttpUtility.UrlEncode(value)
        newqs = "${encKey}=${encVal}"
        qs = builder.Query
        if (builder.Query is not null) and (qs != String.Empty):
            qs = "${qs.Substring(1)}&${newqs}"
        else:
            qs = newqs
        builder.Query = qs

    static def AppendQueryArgs(builder as UriBuilder,
                               args as NameValueCollection):

        if args.Count > 0:
            newqs = CreateQueryString(args)
            qs = builder.Query
            if (builder.Query is not null) and (qs != String.Empty):
                qs = "${qs.Substring(1)}&${newqs}"
            else:
                qs = newqs
            builder.Query = qs
