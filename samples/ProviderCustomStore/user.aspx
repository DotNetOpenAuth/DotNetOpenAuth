<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.Provider" TagPrefix="openid" %>
<html>
<head>
	<openid:IdentityEndpoint ID="IdentityEndpoint20" runat="server" ProviderEndpointUrl="~/Server.aspx"
		XrdsUrl="~/user_xrds.aspx" ProviderVersion="V20" />
	<!-- and for backward compatibility with OpenID 1.x RPs... -->
	<openid:IdentityEndpoint ID="IdentityEndpoint11" runat="server" ProviderEndpointUrl="~/Server.aspx"
		ProviderVersion="V11" />
</head>
<body>
	<p>
		OpenID identity page for <%=Request.QueryString["username"]%>
	</p>
</body>
</html>
