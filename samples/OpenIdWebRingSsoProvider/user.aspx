<%@ Page Language="C#" AutoEventWireup="true" Inherits="OpenIdWebRingSsoProvider.User" EnableSessionState="False"
	CodeBehind="user.aspx.cs" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.Provider.UI" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="openid" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
	<openid:IdentityEndpoint ID="IdentityEndpoint20" runat="server" ProviderEndpointUrl="~/Server.aspx"
		XrdsUrl="~/user_xrds.aspx" ProviderVersion="V20" AutoNormalizeRequest="true"
		OnNormalizeUri="IdentityEndpoint20_NormalizeUri" />
	<!-- and for backward compatibility with OpenID 1.x RPs... -->
	<openid:IdentityEndpoint ID="IdentityEndpoint11" runat="server" ProviderEndpointUrl="~/Server.aspx"
		ProviderVersion="V11" />
</head>
<body>
	<p>
		OpenID identity page for
		<asp:Label runat="server" ID="usernameLabel" EnableViewState="false" />
	</p>
</body>
</html>
