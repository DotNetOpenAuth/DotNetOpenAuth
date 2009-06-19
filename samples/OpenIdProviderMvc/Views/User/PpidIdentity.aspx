<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Identity page
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="HeadContent">
	<op:IdentityEndpoint ID="IdentityEndpoint11" runat="server" ProviderEndpointUrl="~/OpenId/PpidProvider"
		ProviderVersion="V11" />
	<op:IdentityEndpoint ID="IdentityEndpoint20" runat="server" ProviderEndpointUrl="~/OpenId/PpidProvider"
		XrdsUrl="~/User/all/xrds" XrdsAutoAnswer="false" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h2>OpenID identity page </h2>
</asp:Content>
