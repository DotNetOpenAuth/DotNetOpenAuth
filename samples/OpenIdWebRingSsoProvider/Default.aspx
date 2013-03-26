<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="OpenIdWebRingSsoProvider._Default" Async="true" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.UI" Namespace="DotNetOpenAuth" TagPrefix="openid" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<openid:XrdsPublisher ID="XrdsPublisher1" runat="server" XrdsUrl="~/op_xrds.aspx" />
</head>
<body>
	<form id="form1" runat="server">
	<p>
		This sample is of an OpenID Provider that acts within a controlled set of web 
		sites (perhaps all belonging to the same organization).&nbsp; It authenticates 
		the user in its own way (Windows Auth, username/password, InfoCard, X.509, 
		anything), and then sends an automatically OpenID assertion to a limited set of 
		whitelisted RPs without prompting the user.
	</p>
	<p>
		This particular sample uses Windows Authentication so that when the user visits 
		an RP and the RP sends the user to this OP for authentication, the process is 
		completely implicit -- the user never sees the OP.</p>
	</form>
</body>
</html>
