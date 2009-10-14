<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginFrame.aspx.cs" Inherits="WebFormsRelyingParty.LoginFrame" %>

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
</head>
<body>
	<div class="wrapper">
		<p>
			Login with an account you already use!
		</p>
		<ul class="OpenIdProviders">
			<li id="https://me.yahoo.com/"><a href="#">
				<div>
					<div>
						<img src="images/yahoo.gif" />
					</div>
					<div class="ui-widget-overlay">
					</div>
				</div>
			</a></li>
			<li id="https://www.google.com/accounts/o8/id"><a href="#">
				<div>
					<div>
						<img src="images/google.gif" />
					</div>
					<div class="ui-widget-overlay">
					</div>
				</div>
			</a></li>
			<li id="OpenIDButton"><a href="#">
				<div>
					<div>
						<img src="images/openid.gif" />
					</div>
					<div class="ui-widget-overlay">
					</div>
				</div>
			</a></li>
		</ul>
		<form runat="server" method="get" style="display: none" id="OpenIDForm">
		<rp:OpenIdAjaxTextBox runat="server" ID="openid_identifier" />
		<div id="NotMyComputerDiv">
			<span title="Checking this box prevents the identifier you type here from being remembered next time someone comes to this web site from this browser." />
			<input type="checkbox" name="NotMyComputer" id="NotMyComputer" value="1" />
			<label for="NotMyComputer">
				This is <i>not</i> my computer</label>
		</div>
		<!--<div id="state"></div>-->
		</form>
		<div class="helpDoc">
			</p>
			<p>
				If you have logged in previously, click the same button you did last time.
			</p>
			<p>
				If you don't have an account with any of these services, just pick Google.
				They'll help you set up an account.
			</p>
		</div>
	</div>

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
</body>
</html>
