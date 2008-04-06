<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="login.aspx.cs" Inherits="login" ValidateRequest="false" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Login</title>
</head>
<body>
	<form id="Form1" runat="server">
	<h2>
		Login Page
	</h2>
	<asp:Label ID="Label1" runat="server" Text="OpenID Login" />
	<asp:TextBox ID="openIdBox" runat="server" />
	<asp:Button ID="loginButton" runat="server" Text="Login" 
		onclick="loginButton_Click" />
	<br />
	<asp:Label ID="loginFailedLabel" runat="server" EnableViewState="False" Text="Login failed"
		Visible="False" />
	<asp:Label ID="loginCanceledLabel" runat="server" EnableViewState="False" Text="Login canceled"
		Visible="False" />
	</form>
</body>
</html>
