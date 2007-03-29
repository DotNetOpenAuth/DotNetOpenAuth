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
/// Page for handling logins to this server. 
/// </summary>
public partial class login : System.Web.UI.Page
{
    protected void Page_Load(object src, EventArgs e)
    {
            State.Session.CheckExpectedSateIsAvailable();

            String s = Util.ExtractUserName(State.Session.LastRequest.IdentityUrl);
            if (s != null)
            {
                username.Text = s;
                username.Enabled = false;
            }
    }

    protected void Login_Click(Object sender, EventArgs e)
    {
        if (FormsAuthentication.Authenticate(username.Text, password.Text))
            FormsAuthentication.RedirectFromLoginPage(username.Text, true);
        else
            status.InnerHtml += "Invalid Login";
    }
}
