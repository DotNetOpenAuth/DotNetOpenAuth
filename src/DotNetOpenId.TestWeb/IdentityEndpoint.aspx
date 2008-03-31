<%@ Page Language="C#" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.Provider" TagPrefix="openid" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	protected override void OnLoad(EventArgs e) {
		base.OnLoad(e);

		IdentityEndpoint1.ProviderLocalIdentifier = "~/" + Request.QueryString["user"];
		IdentityEndpoint1.ProviderVersion = (DotNetOpenId.ProtocolVersion)
			Enum.Parse(typeof(DotNetOpenId.ProtocolVersion), Request.QueryString["version"]);
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Identity page</title>
	<openid:IdentityEndpoint ID="IdentityEndpoint1" runat="server" 
		ProviderEndpointUrl="~/ProviderEndpoint.aspx" />
</head>
<body>
	<form id="form1" runat="server">
	</form>
</body>
</html>
