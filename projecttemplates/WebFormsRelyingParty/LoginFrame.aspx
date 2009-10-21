<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginFrame.aspx.cs" Inherits="WebFormsRelyingParty.LoginFrame" EnableViewState="false" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<!-- COPYRIGHT (C) 2009 Andrew Arnott.  All rights reserved. -->
<!-- LICENSE: Microsoft Public License available at http://opensource.org/licenses/ms-pl.html -->
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Login</title>
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1" />
	<link type="text/css" href="theme/ui.all.css" rel="Stylesheet" />
	<link href="styles/loginpopup.css" rel="stylesheet" type="text/css" />

	<script type="text/javascript" src="scripts/jquery-1.3.1.js"></script>
	<script type="text/javascript" src="scripts/jquery-ui-personalized-1.6rc6.js"></script>
	<script type="text/javascript" src="scripts/jquery.cookie.js"></script>
	<!--	<script src="http://www.google.com/jsapi"></script>
	<script type="text/javascript" language="javascript">
		google.load("jquery", "1.3.2");
		google.load("jqueryui", "1.7.2");
	</script>
-->
	<script src="scripts/LoginPopup.js" type="text/javascript"></script>
<script>
	//window.openid_visible_iframe = true; // causes the hidden iframe to show up
	//window.openid_trace = true; // causes lots of messages
</script>
</head>
<body>
	<form runat="server" id="form1">
	<div class="wrapper">
		<p>
			Login with an account you already use!
		</p>
		<rp:OpenIdButtonPanel runat="server" OnLoggedIn="openIdButtonPanel_LoggedIn">
			<Providers>
				<rp:ProviderInfo OPIdentifier="https://me.yahoo.com/" Image="images/yahoo.gif" />
				<rp:ProviderInfo OPIdentifier="https://www.google.com/accounts/o8/id" Image="images/google.gif" />
				<%--<rp:ProviderInfo OPIdentifier="https://www.myopenid.com/" Image="images/myopenid.png" />--%>
				<rp:ProviderInfo Image="images/openid.gif" />
			</Providers>
		</rp:OpenIdButtonPanel>
		<asp:HiddenField runat="server" ID="topWindowUrl" />
		<div class="helpDoc">
			<p>
				If you have logged in previously, click the same button you did last time.
			</p>
			<p>
				If you don't have an account with any of these services, just pick Google. They'll
				help you set up an account.
			</p>
		</div>
	</div>
	</form>
</body>
</html>
