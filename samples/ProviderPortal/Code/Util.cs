using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using DotNetOpenId.Provider;
using ProviderPortal;
using DotNetOpenId;

/// <summary>
/// Summary description for Util
/// </summary>
public class Util {
	public static string ExtractUserName(Uri url) {
		return url.Segments[url.Segments.Length - 1];
	}

	public static void GenerateHttpResponse(OpenIdException e) {
		State.Session.Reset();
		StringBuilder text = new StringBuilder();
		foreach (KeyValuePair<string, string> pair in e.EncodedFields)
			text.AppendLine(pair.Key + "=" + pair.Value);
		string error = @"
        <html><head><title>Error Processing Request</title></head><body>
        <p><pre>{0}</pre></p>
        <!--

        This is a large comment.  It exists to make this page larger.
        That is unfortunately necessary because of the 'smart'
        handling of pages returned with an error code in IE.

        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************

        --></body></html>";
		error = String.Format(error, HttpUtility.HtmlEncode(text.ToString()));
		HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
		HttpContext.Current.Response.Write(error);
		HttpContext.Current.Response.Close();
	}
}
