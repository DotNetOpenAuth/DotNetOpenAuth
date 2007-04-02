using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

/// <summary>
/// This page is a required as part of the service discovery phase of the openid protocol (step 1).
/// It simply renders the xml for doing service discovery of server.aspx using the xrds mechanism. 
/// This page is obtained by parsing the user.aspx page.
/// </summary>
public partial class xrds : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "application/xrds+xml";
    }

    /// <summary>
    /// Gets the server URL.
    /// </summary>
    /// <value>The server URL.</value>
    public string ServerUrl
    {
        get
        {
            String path = Response.ApplyAppPathModifier("~/server.aspx"); // ApplyAppPathModifier will convert this path to a fully qualified absolute url
            UriBuilder builder = new UriBuilder(Request.Url);
            builder.Path = path;
            builder.Query = null;
            builder.Fragment = null;
            builder.Port = Convert.ToInt32(ConfigurationManager.AppSettings["ServerPort"]); ;
            return builder.ToString();
        }
    }

}
