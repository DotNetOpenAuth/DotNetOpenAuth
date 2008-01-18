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
		internal const string OpenIdPrefix = "openid.";

		internal const string OpenIdReturnTo = "openid.return_to";
		internal const string OpenIdMode = "openid.mode";
		internal const string OpenIdError = "openid.error";
		internal const string OpenIdIdentity = "openid.identity";
		internal const string OpenIdAssocHandle = "openid.assoc_handle";
		internal const string OpenIdSig = "openid.sig";
		internal const string OpenIdSigned = "openid.signed";
		internal const string OpenIdUserSetupUrl = "openid.user_setup_url";
		internal const string OpenIdTrustRoot = "openid.trust_root";
		internal const string Nonce = "nonce";

		internal const string OpenIdSregPolicyUrl = "openid.sreg.policy_url";
		internal const string OpenIdSregOptional = "openid.sreg.optional";
		internal const string OpenIdSregRequired = "openid.sreg.required";
		internal const string OpenIdSregNickname = "openid.sreg.nickname";
		internal const string OpenIdSregEmail = "openid.sreg.email";
		internal const string OpenIdSregFullname = "openid.sreg.fullname";
		internal const string OpenIdSregDob = "openid.sreg.dob";
		internal const string OpenIdGender = "openid.sreg.gender";
		internal const string OpenIdPostCode = "openid.sreg.postcode";
		internal const string OpenIdCountry = "openid.sreg.country";
		internal const string OpenIdLanguage = "openid.sreg.language";
		internal const string OpenIdTimezone = "openid.sreg.timezone";

		internal static class OpenIdModes {
			internal const string Cancel = "cancel";
			internal const string Error = "error";
			internal const string IdRes = "id_res";
			internal const string CheckIdImmediate = "checkid_immediate";
			internal const string CheckIdSetup = "checkid_setup";
		}
		internal static class OpenIdGenders {
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
