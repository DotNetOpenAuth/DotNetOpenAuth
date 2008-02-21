using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using DotNetOpenId.Provider;
using ProviderPortal;

/// <summary>
/// Summary description for Util
/// </summary>
public class Util {
	public static string ExtractUserName(Uri url) {
		return url.Segments[url.Segments.Length - 1];
	}

	public static void GenerateHttpResponse(ProtocolException e) {
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

	public static void GenerateHttpResponse(DotNetOpenId.Provider.WebResponse webresponse) {
		State.Session.Reset();
		Provider server = new Provider();

		if (webresponse.Code == HttpStatusCode.Redirect) {
			Trace.TraceInformation("Send response as 302 browser redirect to: '{0}'", webresponse.Headers["Location"]);

			HttpContext.Current.Response.Redirect(webresponse.Headers["Location"]);
			return;
		}
		HttpContext.Current.Response.StatusCode = (int)webresponse.Code;
		foreach (string key in webresponse.Headers)
			HttpContext.Current.Response.AddHeader(key, webresponse.Headers[key]);

		if (webresponse.Body != null) {

			Trace.TraceInformation("Send response as server side HTTP response.");
			Trace.Indent();
			Trace.TraceInformation("HTTP Response headers follows: \n{0}",
				Global.ToString(webresponse.Headers));
			Trace.TraceInformation("HTTP Response follows: \n{0}",
				System.Text.Encoding.UTF8.GetString(webresponse.Body));
			Trace.Unindent();

			HttpContext.Current.Response.Write(System.Text.Encoding.UTF8.GetString(webresponse.Body));

		}
		// HttpContext.Current.Response.Flush();
		// HttpContext.Current.Response.Close();
	}

	public static Uri ServerUri {
		get {
			UriBuilder builder = new UriBuilder(HttpContext.Current.Request.Url);
			builder.Path = HttpContext.Current.Response.ApplyAppPathModifier("~/server.aspx");
			builder.Query = null;
			builder.Fragment = null;
			return builder.Uri;
		}
	}
}
