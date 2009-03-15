<%@ Page Title="RP supports Identifier Select" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="IdentifierSelect.aspx.cs" Inherits="RP_IdentifierSelect" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth" TagPrefix="dnoa" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<dnoa:XrdsPublisher ID="XrdsPublisher1" runat="server" XrdsUrl="~/RP/IdentifierSelectXrdsOP.aspx" />
	<op:ProviderEndpoint ID="ProviderEndpoint1" runat='server' OnAuthenticationChallenge="ProviderEndpoint1_AuthenticationChallenge" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:Panel runat="server" EnableViewState="false" Visible="false" ID="AuthFailed">
		<p>The RP has failed the Identifier Select test by not sending the correct Claimed Identifier.
		</p>
	</asp:Panel>
	<h3>Instructions</h3>
	<ol>
		<li>Log into an OpenID RP to be tested using this identifier:
			<%= new Uri(Request.Url, Request.Url.AbsolutePath) %>
		</li>
	</ol>
	<h3>Passing criteria</h3>
	<p>At the conclusion of the steps in the instructions, the RP must recognize that
		<%= new Uri(Request.Url, Page.ResolveUrl("~/RP/IdentifierSelectIdentity.aspx")) %>
		has logged in. </p>
</asp:Content>
