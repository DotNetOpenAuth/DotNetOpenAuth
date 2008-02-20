<%@ Page Language="C#" AutoEventWireup="true" Inherits="user"  Debug="true" Codebehind="user.aspx.cs" %>
<html>
  <head>
    <link rel="openid.server" href="<%=Server.HtmlEncode(ServerUrl)%>" />
  </head>
  <body>
    <p>OpenID identity page for <%=Server.HtmlEncode(UserName)%></p>
  </body>
</html>