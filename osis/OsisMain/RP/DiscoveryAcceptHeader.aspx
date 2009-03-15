<%@ Page Title="RP sends HTTP Accept header for XRDS discovery" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="DiscoveryAcceptHeader.aspx.cs" Inherits="RP_DiscoveryAcceptHeader" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth" TagPrefix="dnoa" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<dnoa:XrdsPublisher ID="XrdsPublisher1" runat="server" XrdsAdvertisement="None" XrdsUrl="~/RP/DiscoveryAcceptHeaderXrds.aspx" />
	<op:ProviderEndpoint ID="ProviderEndpoint1" runat='server' OnAuthenticationChallenge="ProviderEndpoint1_AuthenticationChallenge" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:Panel ID="AuthPanel" runat="server" EnableViewState="false" Visible="false">
		<p>The RP has requested authentication. It has PASSED this test.</p>
	</asp:Panel>
	<h3>Instructions</h3>
	<ol>
		<li>Log into an OpenID RP to be tested using this identifier:
			<%= new Uri(Request.Url, Request.Url.AbsolutePath) %>
		</li>
	</ol>
	<h3>Passing criteria</h3>
	<p>If the RP fails to initiate authentication (most likely claiming that the URL you
		entered is not a valid OpenID Identifier, or that discovery failed), it failed the
		test. If clicking Login at the RP successfully redirects the browser to this page,
		the RP passes the test. </p>
</asp:Content>
