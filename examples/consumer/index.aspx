<%@ Page Language="C#" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
  <title>Welcome to an OpenId enabled ASP Application</title>
  <meta http-equiv="Content-Type" content="text/html; charset=ISO-8859-1" />

  <script runat="server">
    protected override void OnLoad(EventArgs args)
    {
      base.OnLoad(args);
      AuthUser.Text = User.Identity.Name;
    }

    public void OnClick_SignOut(Object src, EventArgs args)
    {
      FormsAuthentication.SignOut();
      Response.Redirect(Request.UrlReferrer.ToString());
    }
</script>

</head>
<body>
  <form runat="server" id="form1">
    <div class="header">
      <h1 class="header">You logged in to this ASP.Net Application via OpenId </h1>
      <h2 class="header">You were successfully authorized as... </h2>
    </div>
    <h3>user:</h3>
    <asp:Label ID="AuthUser" runat="server" />
    <div>
      <asp:Button Text="Sign Out" OnClick="OnClick_SignOut" runat="server" />
    </div>
  </form>
</body>
</html>
