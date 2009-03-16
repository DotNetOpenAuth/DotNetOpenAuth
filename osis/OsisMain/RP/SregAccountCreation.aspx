<%@ Page Title="RP account creation using Simple Registration" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="SregAccountCreation.aspx.cs" Inherits="RP_SregAccountCreation" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/SregAccountCreation.aspx"
		ProviderVersion="V20" />
	<op:ProviderEndpoint ID="ProviderEndpoint1" runat='server' OnAuthenticationChallenge="ProviderEndpoint1_AuthenticationChallenge" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:Panel runat="server" ID="NoSregRequest" EnableViewState="false" Visible="false">
		<p>The RP under test sent an authentication request without including the Simple Registration
			extension. The RP has FAILED this test. </p>
	</asp:Panel>
	<h3>Instructions </h3>
	<ol>
		<li>Log into an RP using the Claimed Identifier: <asp:Label runat="server" Font-Bold="true"
			ID="claimedIdLabel" /></li>
	</ol>
	<h3>Passing criteria </h3>
	<p>The RP passes if authentication completes and the RP account creation process has
		been streamlined using profile values provided by this test, allowing the tester
		to avoid having to provide personal data such as full name, email address, etc.
	</p>
</asp:Content>
