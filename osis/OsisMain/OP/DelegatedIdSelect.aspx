<%@ Page Language="C#" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	protected void Page_Load(object sender, EventArgs e) {
		if (string.IsNullOrEmpty(Request.QueryString["ep"])) {
			throw new ArgumentException("ep parameter missing.");
		}
		IdentityEndpoint1.ProviderEndpointUrl = Request.QueryString["ep"];
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Delegated directed identity simulation</title>
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderLocalIdentifier="http://specs.openid.net/auth/2.0/identifier_select" />
</head>
<body>
	<form id="form1" runat="server">
	<div>
		This is the delegated directed identity simulation page.
	</div>
	</form>
</body>
</html>
