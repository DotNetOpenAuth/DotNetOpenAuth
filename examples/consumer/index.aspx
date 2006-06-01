<%@ Page language="C#" %>
<%@ Import namespace="System.IO" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">
<html>
<head>
<title>Welcome to an OpenId enabled ASP Application</title>
<meta http-equiv="Content-Type" content="text/html; charset=ISO-8859-1">
<script runat="server">
	protected override void OnLoad (EventArgs args)
	{
		base.OnLoad (args);
		AuthUser.Text = User.Identity.Name;
	}

    public void OnClick_SignOut (Object src, EventArgs args)
    {
        FormsAuthentication.SignOut();
	Console.WriteLine("Redirecting to: {0}",
            Request.UrlReferrer.ToString());
        Response.Redirect(Request.UrlReferrer.ToString());
    }
</script>
</head>
<body>
<div class="header">
<h1 class="header">
You logged in to this ASP.Net Application via OpenId
</h1>
<h2 class="header">
You were successfully authorized as...
</h2>
</div>
<form runat="server">
    <h3>user:</h3><asp:label id="AuthUser" runat="server" />
    <div>
    <asp:Button text="Sign Out" OnClick="OnClick_SignOut" runat="server" />
    </div>
</form>
</html>

