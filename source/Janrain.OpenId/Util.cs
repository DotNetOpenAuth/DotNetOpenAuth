using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Janrain.OpenId
{
    public static class UriUtil
    {

        #region NormalizeUri(string uriStr)

        public static Uri NormalizeUri(string uriStr)
        {
            if (!uriStr.StartsWith("http") && uriStr.IndexOf("://") == -1)
                uriStr = "http://" + uriStr;

            UriBuilder bldr = new UriBuilder(uriStr);
            
            bldr.Host = bldr.Host.ToLower();

            return bldr.Uri;
        }

        #endregion

        #region CreateQueryString(NameValueCollection args)

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

        #endregion

        #region AppendQueryArg(UriBuilder builder, string key, string value)

        public static void AppendQueryArg(UriBuilder builder, string key, string value)
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

        #endregion

        #region AppendQueryArgs(UriBuilder builder, NameValueCollection args)

        public static void AppendQueryArgs(UriBuilder builder, NameValueCollection args)
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

        #endregion

    }

    internal static class Util
    {
        internal const string DefaultNamespace = "Janrain.OpenId";
        #region InArray(string[] array, string valueToFind)
            
        public static bool InArray(string[] array, string valueToFind)
        {
            foreach (string val in array)
            {
                if (val == valueToFind) return true;
            }
            return false;
        }
        #endregion
    }

    internal static class QueryStringArgs {
        /// <summary>openid. variables that don't include the "openid." prefix.</summary>
        internal static class openidnp {
            internal const string return_to = "return_to";
            internal const string mode = "mode";
            internal const string error = "error";
            internal const string identity = "identity";
            internal const string expires_in = "expires_in";
            internal const string assoc_type = "assoc_type";
            internal const string assoc_handle = "assoc_handle";
            internal const string session_type = "session_type";
            internal const string is_valid = "is_valid";
            internal const string sig = "sig";
            internal const string signed = "signed";
            internal const string user_setup_url = "user_setup_url";
            internal const string trust_root = "trust_root";
            internal const string invalidate_handle = "invalidate_handle";
            internal const string dh_modulus = "dh_modulus";
            internal const string dh_gen = "dh_gen";
            internal const string dh_consumer_public = "dh_consumer_public";
            internal const string dh_server_public = "dh_server_public";

            internal static class sregnp {
                internal const string policy_url = "policy_url";
                internal const string optional = "optional";
                internal const string required = "required";
                internal const string nickname = "nickname";
                internal const string email = "email";
                internal const string fullname = "fullname";
                internal const string dob = "dob";
                internal const string gender = "gender";
                internal const string postcode = "postcode";
                internal const string country = "country";
                internal const string language = "language";
                internal const string timezone = "timezone";
            }

            internal static class sreg {
                internal const string Prefix = "sreg.";
                internal const string policy_url = Prefix + sregnp.policy_url;
                internal const string optional = Prefix + sregnp.optional;
                internal const string required = Prefix + sregnp.required;
                internal const string nickname = Prefix + sregnp.nickname;
                internal const string email = Prefix + sregnp.email;
                internal const string fullname = Prefix + sregnp.fullname;
                internal const string dob = Prefix + sregnp.dob;
                internal const string gender = Prefix + sregnp.gender;
                internal const string postcode = Prefix + sregnp.postcode;
                internal const string country = Prefix + sregnp.country;
                internal const string language = Prefix + sregnp.language;
                internal const string timezone = Prefix + sregnp.timezone;
            }

        }
        /// <summary>openid. variables that include the "openid." prefix.</summary>
        internal static class openid {
            internal const string Prefix = "openid.";

            internal const string return_to = Prefix + openidnp.return_to;
            internal const string mode = Prefix + openidnp.mode;
            internal const string error = Prefix + openidnp.error;
            internal const string identity = Prefix + openidnp.identity;
            internal const string expires_in = Prefix + openidnp.expires_in;
            internal const string assoc_type = Prefix + openidnp.assoc_type;
            internal const string assoc_handle = Prefix + openidnp.assoc_handle;
            internal const string session_type = Prefix + openidnp.session_type;
            internal const string is_valid = Prefix + openidnp.is_valid;
            internal const string sig = Prefix + openidnp.sig;
            internal const string signed = Prefix + openidnp.signed;
            internal const string user_setup_url = Prefix + openidnp.user_setup_url;
            internal const string trust_root = Prefix + openidnp.trust_root;
            internal const string invalidate_handle = Prefix + openidnp.invalidate_handle;
            internal const string dh_modulus = Prefix + openidnp.dh_modulus;
            internal const string dh_gen = Prefix + openidnp.dh_gen;
            internal const string dh_consumer_public = Prefix + openidnp.dh_consumer_public;
            internal const string dh_server_public = Prefix + openidnp.dh_server_public;

            internal static class sreg {
                internal const string Prefix = openid.Prefix + openidnp.sreg.Prefix;
                internal const string policy_url = Prefix + openidnp.sregnp.policy_url;
                internal const string optional = Prefix + openidnp.sregnp.optional;
                internal const string required = Prefix + openidnp.sregnp.required;
                internal const string nickname = Prefix + openidnp.sregnp.nickname;
                internal const string email = Prefix + openidnp.sregnp.email;
                internal const string fullname = Prefix + openidnp.sregnp.fullname;
                internal const string dob = Prefix + openidnp.sregnp.dob;
                internal const string gender = Prefix + openidnp.sregnp.gender;
                internal const string postcode = Prefix + openidnp.sregnp.postcode;
                internal const string country = Prefix + openidnp.sregnp.country;
                internal const string language = Prefix + openidnp.sregnp.language;
                internal const string timezone = Prefix + openidnp.sregnp.timezone;
            }
        }
        internal const string nonce = "nonce";
        internal const string enc_mac_key = "enc_mac_key";
        internal const string mac_key = "mac_key";
        internal const string DH_SHA1 = "DH-SHA1";
        internal const string HMAC_SHA1 = "HMAC-SHA1";

        internal static class Modes {
            internal const string cancel = "cancel";
            internal const string error = "error";
            internal const string id_res = "id_res";
            internal const string checkid_immediate = "checkid_immediate";
            internal const string checkid_setup = "checkid_setup";
            internal const string check_authentication = "check_authentication";
            internal const string associate = "associate";
        }
        internal static class Genders {
            internal const string Male = "M";
            internal const string Female = "F";
        }

        /// <summary>
        /// Used by ASP.NET on a login page to determine where a successful
        /// login should be redirected to.
        /// </summary>
        internal const string ReturnUrl = "ReturnUrl";
    }
}
