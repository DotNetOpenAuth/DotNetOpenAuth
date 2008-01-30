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
        State.Session.CheckExpectedStateIsAvailable();

        if (!IsPostBack)
        {
            username.Text = Util.ExtractUserName(State.Session.LastRequest.IdentityUrl);
            password.Focus();
        }
    }

    protected void Login_Click(Object sender, EventArgs e)
    {
        // Don't use username from text field because the user may have hijacked and changed it.
        string challengedUsername = Util.ExtractUserName(State.Session.LastRequest.IdentityUrl);
        if (FormsAuthentication.Authenticate(challengedUsername, password.Text))
            FormsAuthentication.RedirectFromLoginPage(username.Text, true);
        else
            status.InnerHtml += "Invalid Login";
    }
}
