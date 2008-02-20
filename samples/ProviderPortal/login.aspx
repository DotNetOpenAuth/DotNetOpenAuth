<%@ Page Language="C#" AutoEventWireup="true" Inherits="login" Debug="true" Codebehind="login.aspx.cs" %>
<html>
<head>
    <title>Login</title>
</head>
<body>
    <p class=title>Login</p> 
    <span id="status" class="text" enableviewstate="false" runat="Server"/>
   <br />Try Bob/Test. Usernames are defined in the web.config 
    <form id="Form1" runat="server">
    Username: <asp:textbox id=username ReadOnly=true cssclass="text" runat="Server"/><br/>
    Password: <asp:textbox id=password textmode=Password cssclass="text" runat="Server"/><br />
    <asp:button id=login_button onclick="Login_Click" text="  Login  " cssclass="button" runat="Server"/>
    </form>
</body>
</html>
