<%@ Page Language="C#" %>
<html>
  <head>
    <script runat="server">
public string ServerUrl
{
    get {
        String path = Response.ApplyAppPathModifier("~/server.aspx");
        UriBuilder builder = new UriBuilder(Request.Url);
        builder.Path = path;
        builder.Query = null;
        builder.Fragment = null;
        return builder.ToString();
    }
}

public string UserName
{
    get {
        return Request.QueryString["name"];
    }
}

public string XrdsUrl
{
    get {
        String path = Response.ApplyAppPathModifier("~/xrds/" + UserName);
        UriBuilder builder = new UriBuilder(Request.Url);
        builder.Path = path;
        builder.Query = null;
        builder.Fragment = null;
        return builder.ToString();
    }
}
    </script><meta http-equiv="X-XRDS-Location" content="<%=Server.HtmlEncode(XrdsUrl)%>" />
    <link rel="openid.server" href="<%=Server.HtmlEncode(ServerUrl)%>" />
  </head>
  <body>
    <p>OpenID identity page for <%=Server.HtmlEncode(UserName)%></p>
  </body>
</html>