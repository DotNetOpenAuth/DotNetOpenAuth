<%@ Page Language="C#" AutoEventWireup="true" CodeFile="login.aspx.cs" Inherits="login" %>

<%@ Register Assembly="Janrain.OpenId" Namespace="NerdBank.OpenId.Consumer" TagPrefix="cc1" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">

<html>
<head>
  <title>Login</title>
    <link href="StyleSheet.css" rel="stylesheet" type="text/css" />
</head>
<body>
  <form id="Form1" runat="server">

    <h2>Login Page</h2>
    <cc1:OpenIdLogin ID="OpenIdLogin1" runat="server" CssClass="openid_login" OnLoggedIn="OpenIdLogin1_LoggedIn" RequestCountry="Request" RequestEmail="Request" RequestGender="Require" RequestPostalCode="Require" RequestTimeZone="Require" />
        &nbsp;&nbsp;<br />
    
    <br/>
    &nbsp;
  </form>
</body>
</html>
