<%@ Page Language="C#" AutoEventWireup="true" UICulture="en" %>

<%-- The UICulture="en" tests for regressions on GoogleCode Issue 60. --%>
<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId" TagPrefix="openid" %>
<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.RelyingParty" TagPrefix="rp" %>

<script runat="server">
	protected void Page_Load(object sender, EventArgs e) {
		if (Request.QueryString["AllowRPDiscovery"] == "false") {
			xrdsPublisher.Enabled = false;
		}
	}
	
	protected void loginButton_Click(object sender, EventArgs e) {
		OpenIdTextBox1.LogOn();
	}
</script>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Untitled Page</title>
	<openid:XrdsPublisher runat="server" ID="xrdsPublisher" XrdsUrl="~/rp_xrds.aspx" />
</head>
<body>
	<form id="form1" runat="server">
	<rp:OpenIdTextBox ID="OpenIdTextBox1" runat="server" />
	<asp:Button ID="loginButton" runat="server" OnClick="loginButton_Click" Text="Login" />
	</form>
</body>
</html>
