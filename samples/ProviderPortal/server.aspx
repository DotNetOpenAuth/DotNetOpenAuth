<%@ Page Language="C#" AutoEventWireup="true" Inherits="server" Debug="true" Codebehind="server.aspx.cs" %>
<asp:placeholder runat="server" id="contentForWebBrowsers" visible="false" > 
<html>
  <head>
    <title>This is an OpenID server</title>
  </head>
  <body>
    <p><%=Request.Url%> is an OpenID server endpoint.</p>
    <p>For more information about OpenID, see:</p>
    <dl>
      <dt>http://www.openidenabled.com/</dt>
      <dd>An OpenID community Web site, home of this library</dd>
      <dt>http://www.openid.net/</dt><dd>the official OpenID Web site</dd>
    </dl>
  </body>
</html>
<%
    int i = 1;
%>
</asp:placeholder>
