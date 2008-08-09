<%@ Page Language="C#" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId" TagPrefix="openid" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Untitled Page</title>
	<openid:XrdsPublisher runat="server" ID="xrdsPublisher" XrdsUrl="~/rp_xrds.aspx" XrdsAdvertisement=Both />
</head>
<body>
	<form id="form1" runat="server">
	Test home page, used for RP discovery.
	</form>
</body>
</html>
