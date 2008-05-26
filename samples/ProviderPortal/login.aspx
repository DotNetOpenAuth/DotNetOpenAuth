<%@ Page Language="C#" AutoEventWireup="true" Inherits="login" CodeBehind="login.aspx.cs" %>

<html>
<head>
	<title>Login</title>
</head>
<body>
	<form id="Form1" runat="server">
	<h1>
		OpenID Provider Login
	</h1>
	<p>
		Usernames are defined in the App_Data\Users.xml file.
	</p>
	<asp:Login runat="server" ID="login1" />
	
	<p>Credentials to try (each with their own OpenID)</p>
	<table>
		<tr><td>Username</td><td>Password</td></tr>
		<tr><td>bob</td><td>test</td></tr>
		<tr><td>bob1</td><td>test</td></tr>
		<tr><td>bob2</td><td>test</td></tr>
		<tr><td>bob3</td><td>test</td></tr>
		<tr><td>bob4</td><td>test</td></tr>
	</table>
	</form>
</body>
</html>
