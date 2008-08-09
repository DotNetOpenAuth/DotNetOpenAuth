<%@ Page Language="C#" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId" TagPrefix="openid" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	protected override void OnLoad(EventArgs e) {
		base.OnLoad(e);

		xrdsPublisher.XrdsUrl += "?user=" + Request.QueryString["user"];
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Untitled Page</title>
	<!-- this page is the directed identity OP Identifier -->
	<openid:XrdsPublisher runat="server" ID="xrdsPublisher" XrdsUrl="~/op_xrds.aspx" EnableViewState="false" />
</head>
<body>
	<form id="form1" runat="server">
	Test home page, used for OP discovery.
	</form>
</body>
</html>
