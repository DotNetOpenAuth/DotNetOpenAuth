<%@ Page Language="C#" AutoEventWireup="true" CodeFile="user.aspx.cs" Inherits="user"  Debug="true"%>
<html>
  <head>
    <link rel="openid.server" href="<%=Server.HtmlEncode(ServerUrl)%>" />
  </head>
  <body>
    <p>OpenID identity page for <%=Server.HtmlEncode(UserName)%></p>
  </body>
</html>