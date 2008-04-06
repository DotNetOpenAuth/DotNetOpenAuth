<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="login.aspx.cs" Inherits="login" ValidateRequest="false" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.RelyingParty" TagPrefix="cc1" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Login</title>
</head>
<body>
	<form id="Form1" runat="server">
	<h2>
		Login Page </h2>
	<cc1:OpenIdLogin ID="OpenIdLogin1" runat="server" CssClass="openid_login" RequestCountry="Request"
		RequestEmail="Request" RequestGender="Require" RequestPostalCode="Require" RequestTimeZone="Require"
		RememberMeVisible="True" PolicyUrl="~/PrivacyPolicy.aspx" TabIndex="1" OnLoggedIn="OpenIdLogin1_LoggedIn"
		OnCanceled="OpenIdLogin1_Canceled" OnFailed="OpenIdLogin1_Failed" OnSetupRequired="OpenIdLogin1_SetupRequired" />
	<asp:CheckBox ID="immediateCheckBox" runat="server" Text="Immediate mode" />
	<br />
	<asp:Label ID="loginFailedLabel" runat="server" EnableViewState="False" Text="Login failed"
		Visible="False" />
	<asp:Label ID="loginCanceledLabel" runat="server" EnableViewState="False" Text="Login canceled"
		Visible="False" />
	<p>
		<asp:ImageButton runat="server" ImageUrl="~/images/yahoo.png" ID="yahooLoginButton" OnClick="yahooLoginButton_Click" />
	</p>
	</form>
</body>
</html>
