<%@ Page Language="C#" AutoEventWireup="true" Inherits="user" Debug="true" CodeBehind="user.aspx.cs" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.Provider" TagPrefix="openid" %>
<html>
<head>
	<openid:IdentityEndpoint runat="server" ServerUrl="~/Server.aspx" />
</head>
<body>
	<p>
		OpenID identity page for
		<asp:Label runat="server" ID="usernameLabel" EnableViewState="false" />
	</p>
</body>
</html>
