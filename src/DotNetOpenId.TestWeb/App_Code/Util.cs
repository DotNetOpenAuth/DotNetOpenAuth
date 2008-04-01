using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

/// <summary>
/// Summary description for Utilcs
/// </summary>
public static class Util {
	public static Uri GetFullUrl(string relativeUri) {
		return new Uri(HttpContext.Current.Request.Url, 
			HttpContext.Current.Response.ApplyAppPathModifier(relativeUri));
	}
}
