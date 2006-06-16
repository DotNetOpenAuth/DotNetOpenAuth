<%@Page Language="C#" %>
<script runat="server">
    protected void Page_Load(object src, EventArgs e) {
        Janrain.OpenId.Server.CheckIdRequest request = (Janrain.OpenId.Server.CheckIdRequest) Session["last_request"];
        
        if (request != null)
        {
            String s = Janrain.OpenId.Server.Asp.ServerHttpModule.ExtractUserName(request.IdentityUrl, Request);
            if (s != null)
            {
                username.Text = s;
                username.Enabled = false;
            }
        }
    }

    void Login_Click(Object sender, EventArgs e) {
        if (FormsAuthentication.Authenticate(username.Text, password.Text))
            FormsAuthentication.RedirectFromLoginPage(username.Text, true);
        else
            status.InnerHtml += "Invalid Login";
    }
</script><html>
<head>
    <title>Login</title>
</head>
<body>
    <p class=title>Login</p> 
    <span id="status" class="text" runat="Server"/>
    <form runat="server">
    Username: <asp:textbox id=username cssclass="text" runat="Server"/><br/>
    Password: <asp:textbox id=password textmode=Password cssclass="text" runat="Server"/><br />
    <asp:button id=login_button onclick="Login_Click" text="  Login  " cssclass="button" runat="Server"/>
    </form>
</body>
</html>
