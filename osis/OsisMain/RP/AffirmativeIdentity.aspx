<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
	CodeFile="AffirmativeIdentity.aspx.cs" Inherits="RP_AffirmativeIdentity" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content runat="server" ContentPlaceHolderID="head">
	<!-- This identity page doubles as the OP endpoint -->
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/AffirmativeIdentity.aspx"
		ProviderVersion="V20" />
	<op:ProviderEndpoint ID="ProviderEndpoint1" runat='server' OnAuthenticationChallenge="ProviderEndpoint1_AuthenticationChallenge" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<p>This is an identity page that always provides a positive assertion with no authentication
		effort on the part of the user. </p>
</asp:Content>
