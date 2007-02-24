using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Janrain.OpenId
{
    public class Util
    {

        private Util() { }

        public static Uri NormalizeUri(string uriStr)
        {
            if (!uriStr.StartsWith("http") && uriStr.IndexOf("://") == -1)
                uriStr = "http://" + uriStr;

            UriBuilder bldr = new UriBuilder(uriStr);
            
            bldr.Host = bldr.Host.ToLower();

            return bldr.Uri;
        }

        public static string CreateQueryString(NameValueCollection args)
        {
            string[] parts = new string[args.Count];
            
            for (int i = 0; i < args.Count; i++)
            {
                string encKey = HttpUtility.UrlEncode(args.GetKey(i));
                string encVal = HttpUtility.UrlEncode(args.Get(i));

                parts[i] = encKey + "=" + encVal;
            }

            return String.Join("&", parts);
        }

        public static void AppendQueryArg(ref UriBuilder builder, string key, string value)
        {
            string encKey = HttpUtility.UrlEncode(key);
            string encVal = HttpUtility.UrlEncode(value);
            string newqs = encKey + "=" + encVal;
            string qs = builder.Query;

            if (builder.Query != null && qs != String.Empty)
                qs = qs.Substring(1) + "&" + newqs;
            else
                qs = newqs;

            builder.Query = qs;

        }

        public static void AppendQueryArgs(ref UriBuilder builder, NameValueCollection args)
        {
            if (args.Count > 0)
            {
                string newqs = CreateQueryString(args);
                string qs = builder.Query;

                if (builder.Query != null && qs != String.Empty)
                    qs = qs.Substring(1) + "&" + newqs;
                else
                    qs = newqs;

                builder.Query = qs;
            }
        }

    }
}
