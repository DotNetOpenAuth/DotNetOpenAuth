//-----------------------------------------------------------------------
// <copyright file="UriHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet {
	using System;
	using System.Text.RegularExpressions;
	using System.Web;
	using DotNetOpenAuth.Messaging;

	internal static class UriHelper {
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
		public static Uri AttachQueryStringParameter(this Uri url, string parameterName, string parameterValue) {
			UriBuilder builder = new UriBuilder(url);
			string query = builder.Query;
			if (query.Length > 1) {
				// remove the '?' character in front of the query string
				query = query.Substring(1);
			}

			string parameterPrefix = parameterName + "=";

			string encodedParameterValue = Uri.EscapeDataString(parameterValue);

			string newQuery = Regex.Replace(query, parameterPrefix + "[^\\&]*", parameterPrefix + encodedParameterValue);
			if (newQuery == query) {
				if (newQuery.Length > 0) {
					newQuery += "&";
				}
				newQuery = newQuery + parameterPrefix + encodedParameterValue;
			}
			builder.Query = newQuery;

			return builder.Uri;
		}

		/// <summary>
		/// Converts an app-relative url, e.g. ~/Content/Return.cshtml, to a full-blown url, e.g. http://mysite.com/Content/Return.cshtml
		/// </summary>
		/// <param name="returnUrl">The return URL.</param>
		/// <returns></returns>
		public static Uri ConvertToAbsoluteUri(string returnUrl, HttpContextBase context) {
			if (Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute)) {
				return new Uri(returnUrl, UriKind.Absolute);
			}

			if (!VirtualPathUtility.IsAbsolute(returnUrl)) {
				returnUrl = VirtualPathUtility.ToAbsolute(returnUrl);
			}

			Uri publicUrl = HttpRequestInfo.GetPublicFacingUrl(context.Request, context.Request.ServerVariables);
			return new Uri(publicUrl, returnUrl);
		}
	}
}