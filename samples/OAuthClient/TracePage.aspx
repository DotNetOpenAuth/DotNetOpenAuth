<%@ Page Language="C#" AutoEventWireup="true" Inherits="OAuthClient.TracePage" Codebehind="TracePage.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
</head>
<body>
	<form id="form1" runat="server">
	<p align="right">
		<asp:Button runat="server" Text="Clear log" ID="clearLogButton" OnClick="clearLogButton_Click" />
	</p>
	<pre>
		<asp:PlaceHolder runat="server" ID="placeHolder1" />
	</pre>
	</form>
</body>
</html>
