<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId" TagPrefix="openid" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
	<title>OpenID Relying Party, by DotNetOpenId</title>
	<openid:XrdsPublisher runat="server" XrdsUrl="~/xrds.aspx" />
</head>
<body>
	<form id="form1" runat="server">
	<h1>
		OpenID Relying Party, with custom store
	</h1>
	<h2>
		Provided by <a href="http://dotnetopenid.googlecode.com">DotNetOpenId</a>
	</h2>
	<p>
		This sample implements a custom store for associations and nonces, which can be useful
		when deploying a relying party site on a web farm.
	</p>
	<p>
		Visit the
		<asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/MembersOnly/Default.aspx"
			Text="Members Only" />
		area. (This will trigger a login demo).
	</p>
	</form>
</body>
</html>
