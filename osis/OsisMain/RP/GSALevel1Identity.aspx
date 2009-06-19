<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/GSALevel1.aspx"
		ProviderVersion="V20" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<p>This is the identity page used for the <asp:HyperLink runat="server" NavigateUrl="~/RP/GSALevel1.aspx"
		Text="GSA Level 1 RP" /> test. </p>
</asp:Content>
