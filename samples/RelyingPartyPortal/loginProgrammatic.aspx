<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="loginProgrammatic.aspx.cs"
	Inherits="loginProgrammatic" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Login</title>
</head>
<body>
	<form id="Form1" runat="server">
	<h2>Login Page </h2>
	<asp:Label ID="Label1" runat="server" Text="OpenID Login" />
	<asp:TextBox ID="openIdBox" runat="server" />
	<asp:Button ID="loginButton" runat="server" Text="Login" OnClick="loginButton_Click" />
	<asp:CustomValidator runat="server" ID="openidValidator" ErrorMessage="Invalid OpenID Identifier"
		ControlToValidate="openIdBox" EnableViewState="false" OnServerValidate="openidValidator_ServerValidate" />
	<br />
	<asp:Label ID="loginFailedLabel" runat="server" EnableViewState="False" Text="Login failed"
		Visible="False" />
	<asp:Label ID="loginCanceledLabel" runat="server" EnableViewState="False" Text="Login canceled"
		Visible="False" />
	</form>
</body>
</html>
