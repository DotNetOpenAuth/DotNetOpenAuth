<%@ Page language="C#" %>
<%@ Register Assembly="NerdBank.OpenId.AspNet" Namespace="NerdBank.OpenId" TagPrefix="openid" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">
<html>
<head>
  <title>Login</title>
</head>
<body>
  <form runat="server">
    <h2>Login Page</h2>
    <openid:OpenIdLogin ID="OpenIdLogin1" runat="server" />
  </form>
</body>
</html>
