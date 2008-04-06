<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId" TagPrefix="openid" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<openid:XrdsPublisher runat="server" XrdsUrl="~/op_xrds.aspx" />
	<title>OpenID Provider, by DotNetOpenId</title>
</head>
<body>
	<form id="form1" runat="server">
	<h1>
		OpenID Provider, with custom store
	</h1>
	<h2>
		Provided by <a href="http://dotnetopenid.googlecode.com">DotNetOpenId</a>
	</h2>
	<p>
		This sample implements a custom store for associations, which can be useful when
		deploying an OpenId provider site on a web farm.
	</p>
	<p>
		This is a very stripped-down sample. No login is required on this site as it automatically
		responds affirmatively to any OpenId request sent to it. Start the authentication
		process on the Relying Party sample site.
	</p>
	</form>
</body>
</html>
