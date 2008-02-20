<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="login.aspx.cs" Inherits="login" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.Consumer" TagPrefix="cc1" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
  <title>Login</title>
</head>
<body>
  <form id="Form1" runat="server">

    <h2>Login Page</h2>
    <cc1:OpenIdLogin ID="OpenIdLogin1" runat="server" CssClass="openid_login" 
        RequestCountry="Request" RequestEmail="Request" RequestGender="Require" 
        RequestPostalCode="Require" RequestTimeZone="Require"
        RememberMeVisible="True" TabIndex="1"
        OnLoggedIn="OpenIdLogin1_LoggedIn" OnCanceled="OpenIdLogin1_Canceled"
        OnError="OpenIdLogin1_Error" />
    <br />
    <asp:Label ID="loginFailedLabel" runat="server" EnableViewState="False" Text="Login failed"
        Visible="False" />
    <asp:Label ID="loginCanceledLabel" runat="server" EnableViewState="False" Text="Login canceled"
        Visible="False" />

  </form>
</body>
</html>
