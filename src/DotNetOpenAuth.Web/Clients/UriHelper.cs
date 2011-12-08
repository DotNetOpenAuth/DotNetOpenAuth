using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using DotNetOpenAuth.Messaging;

namespace DotNetOpenAuth.Web.Clients
{
    internal static class UriHelper
    {
        /// <summary>
        /// Attaches the query string '__provider' to an existing url. If the url already 
        /// contains the __provider query string, it overrides it with the specified provider name.
        /// </summary>
        /// <param name="url">The original url.</param>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns>The new url with the provider name query string attached</returns>
        /// <example>
        /// If the url is: http://contoso.com, and providerName='facebook', the returned value is: http://contoso.com?__provider=facebook
        /// If the url is: http://contoso.com?a=1, and providerName='twitter', the returned value is: http://contoso.com?a=1&__provider=twitter
        /// If the url is: http://contoso.com?a=1&__provider=twitter, and providerName='linkedin', the returned value is: http://contoso.com?a=1&__provider=linkedin
        /// </example>
        /// <remarks>
        /// The reason we have to do this is so that when the external service provider forwards user 
        /// back to our site, we know which provider it comes back from.
        /// </remarks>
        public static Uri AttachQueryStringParameter(this Uri url, string parameterName, string parameterValue)
        {
            UriBuilder builder = new UriBuilder(url);
            string query = builder.Query;
            if (query.Length > 1)
            {
                // remove the '?' character in front of the query string
                query = query.Substring(1);
            }

            string parameterPrefix = parameterName + "=";

            string encodedParameterValue = Uri.EscapeDataString(parameterValue);

            string newQuery = Regex.Replace(query, parameterPrefix + "[^\\&]*", parameterPrefix + encodedParameterValue);
            if (newQuery == query)
            {
                if (newQuery.Length > 0)
                {
                    newQuery += "&";
                }
                newQuery = newQuery + parameterPrefix + encodedParameterValue;
            }
            builder.Query = newQuery;

            return builder.Uri;
        }

        /// <summary>
        /// Appends the specified key/value pairs as query string parameters to the builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="pairs">The pairs.</param>
        /// <returns></returns>
        public static void AppendQueryArguments(this UriBuilder builder, IDictionary<string, string> pairs)
        {
            if (pairs == null)
            {
                throw new ArgumentNullException("pairs");
            }

            if (!pairs.Any())
            {
                return;
            }

            string query = builder.Query;
            if (query.Length > 1)
            {
                // remove the '?' character in front of the query string and append the '&'
                query = query.Substring(1);
            }

            var sb = new StringBuilder(query);
            foreach (KeyValuePair<string, string> pair in pairs)
            {
                if (sb.Length > 0)
                {
                    sb.Append('&');
                }
                sb.AppendFormat("{0}={1}", pair.Key, pair.Value);
            }

            builder.Query = sb.ToString();
        }

        /// <summary>
        /// Converts an app-relative url, e.g. ~/Content/Return.cshtml, to a full-blown url, e.g. http://mysite.com/Content/Return.cshtml
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        public static Uri ConvertToAbsoluteUri(string returnUrl)
        {
            if (Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute))
            {
                return new Uri(returnUrl, UriKind.Absolute);
            }

            if (HttpContext.Current == null)
            {
                return null;
            }

            if (!VirtualPathUtility.IsAbsolute(returnUrl))
            {
                returnUrl = VirtualPathUtility.ToAbsolute(returnUrl);
            }

            return new Uri(GetPublicFacingUrl(new HttpRequestWrapper(HttpContext.Current.Request)), returnUrl);
        }

        /// <summary>
        /// Gets the public facing URL of this request as what clients see it.
        /// </summary>
        /// <param name="request">The request.</param>
        public static Uri GetPublicFacingUrl(HttpRequestBase request)
        {
            NameValueCollection serverVariables = request.ServerVariables;
            if (serverVariables["HTTP_HOST"] != null)
            {
                string forwardProto = serverVariables["HTTP_X_FORWARDED_PROTO"];
                if (forwardProto == null)
                {
                    string scheme = request.Url.Scheme;
                    var hostAndPort = new Uri(scheme + Uri.SchemeDelimiter + serverVariables["HTTP_HOST"]);
                    var publicRequestUri = new UriBuilder(request.Url)
                                               {
                                                   Scheme = scheme,
                                                   Host = hostAndPort.Host,
                                                   Port = hostAndPort.Port
                                               };

                    return publicRequestUri.Uri;
                }
            }
            return new Uri(request.Url, request.RawUrl);
        }
    }
}