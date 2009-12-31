<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="OpenIdWebRingSsoProvider._Default" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth" TagPrefix="openid" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<openid:XrdsPublisher ID="XrdsPublisher1" runat="server" XrdsUrl="~/op_xrds.aspx" />
</head>
<body>
	<form id="form1" runat="server">
	<div>
		Provider SSO home page.
	</div>
	</form>
</body>
</html>
