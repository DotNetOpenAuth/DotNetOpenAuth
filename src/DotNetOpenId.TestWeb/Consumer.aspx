<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Consumer.aspx.cs" Inherits="Consumer" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.Consumer" TagPrefix="openid" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Untitled Page</title>
</head>
<body>
	<form id="form1" runat="server">
	<openid:OpenIdTextBox ID="OpenIdTextBox1" runat="server" />
	</form>
</body>
</html>
