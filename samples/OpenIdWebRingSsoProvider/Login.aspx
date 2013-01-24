<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="OpenIdWebRingSsoProvider.Login" Async="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
</head>
<body>
	<form id="form1" runat="server">
	<p>
		Usernames are defined in the App_Data\Users.xml file.
	</p>
	<div style="display: none" id="loginDiv">
		<asp:Login runat="server" ID="login1" />
	</div>
	<div id="javascriptDisabled">
		<b>Javascript appears to be disabled in your browser. </b>This page requires Javascript
		to be enabled to better protect your security.
	</div>
	<p>
		<asp:Button ID="cancelButton" runat="server" onclick="cancelButton_Click" 
			Text="Cancel Login" />
	</p>
	<p>Credentials to try (each with their own OpenID)</p>
	<table>
		<tr><td>Username</td><td>Password</td></tr>
		<tr><td>bob</td><td>test</td></tr>
		<tr><td>bob1</td><td>test</td></tr>
		<tr><td>bob2</td><td>test</td></tr>
		<tr><td>bob3</td><td>test</td></tr>
		<tr><td>bob4</td><td>test</td></tr>
	</table>
		<script language="javascript" type="text/javascript">
			//<![CDATA[
			// we use HTML to hide the action buttons and Javascript to show them
			// to protect against click-jacking in an iframe whose javascript is disabled.
			document.getElementById('loginDiv').style.display = 'block';
			document.getElementById('javascriptDisabled').style.display = 'none';

			// Frame busting code (to protect us from being hosted in an iframe).
			// This protects us from click-jacking.
			if (document.location !== window.top.location) {
				window.top.location = document.location;
			}
			//]]>
		</script>
	</form>
</body>
</html>
