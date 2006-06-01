<%@ Page language="C#" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">
<html>
<head>
  <title>Login</title>
</head>
<body>
  <form runat="server">
    <%
      if (Context.Items["errmsg"] != null)
      {
          Response.Write("<div style=\"color:red;\">");
          Response.Write(Context.Items["errmsg"]);
	  Response.Write("</div>");
      }
    %>
    <h2>Login Page</h2>
    URL:
    <asp:TextBox id="openid_url" runat="server"/>
    <br/>
    <asp:Button id="btnSignIn" text="Login" runat="server" />
  </form>
</body>
</html>
