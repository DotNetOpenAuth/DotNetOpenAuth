<%@ Page Language="C#" AutoEventWireup="true" Inherits="user" CodeBehind="user.aspx.cs" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.Provider" TagPrefix="openid" %>
<html>
<head>
	<openid:IdentityEndpoint runat="server" ProviderEndpointUrl="~/Server.aspx"
		XrdsUrl="~/user_xrds.aspx" ProviderVersion="V20" />
</head>
<body>
	<p>
		OpenID identity page for
		<asp:Label runat="server" ID="usernameLabel" EnableViewState="false" />
	</p>
</body>
</html>
