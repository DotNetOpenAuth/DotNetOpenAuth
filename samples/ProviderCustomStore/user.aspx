<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.Provider" TagPrefix="openid" %>
<html>
<head>
	<openid:IdentityEndpoint runat="server" ProviderEndpointUrl="~/Server.aspx"
		XrdsUrl="~/user_xrds.aspx" ProviderVersion="V20" />
</head>
<body>
	<p>
		OpenID identity page for <%=Request.QueryString["username"]%>
	</p>
</body>
</html>
