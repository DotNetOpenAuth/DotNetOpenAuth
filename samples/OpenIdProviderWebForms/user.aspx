<%@ Page Language="C#" EnableSessionState="False" AutoEventWireup="true" Inherits="OpenIdProviderWebForms.user" CodeBehind="user.aspx.cs" MasterPageFile="~/Site.Master" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.Provider.UI" Namespace="DotNetOpenAuth.OpenId.Provider" TagPrefix="openid" %>
<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="head">
	<openid:IdentityEndpoint ID="IdentityEndpoint20" runat="server" ProviderEndpointUrl="~/Server.aspx"
		XrdsUrl="~/user_xrds.aspx" ProviderVersion="V20" 
		AutoNormalizeRequest="true" OnNormalizeUri="IdentityEndpoint20_NormalizeUri" />
	<!-- and for backward compatibility with OpenID 1.x RPs... -->
	<openid:IdentityEndpoint ID="IdentityEndpoint11" runat="server" ProviderEndpointUrl="~/Server.aspx"
		ProviderVersion="V11" />
</asp:Content>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
	<p>
		OpenID identity page for
		<asp:Label runat="server" ID="usernameLabel" EnableViewState="false" />
	</p>
</asp:Content>