<%@ Page Title="RP verifies assertions against discovery results" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="VerifyAssertionDiscovery.aspx.cs" Inherits="RP_VerifyAssertionDiscovery" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<!-- this page does triple-duty: it's a test instruction page, an identity page, and an OP endpoint -->
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/VerifyAssertionDiscovery.aspx"
		ProviderVersion="V20" ProviderLocalIdentifier="~/RP/VerifyAssertionDiscovery.aspx" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:Panel ID="AuthPanel" runat="server" EnableViewState="false" Visible="false">
		<p>The RP has requested authentication. Please choose a way to meddle with the asserted
			info.</p>
		<h4>Tests that should fail authentication </h4>
		<asp:Button Text="Claimed Identifier (significant)" CommandArgument="1" OnClick="CompleteAuthentication_Click"
			runat="server" />
		<asp:Button Text="Claimed Identifier (capitalization only)" CommandArgument="2" OnClick="CompleteAuthentication_Click"
			runat="server" />
		<asp:Button Text="OP Local Identifier (significant)" CommandArgument="3" OnClick="CompleteAuthentication_Click"
			runat="server" />
		<asp:Button Text="OP Local Identifier (capitalization)" CommandArgument="4" OnClick="CompleteAuthentication_Click"
			runat="server" />
		<asp:Button Text="Provider Endpoint (significant)" CommandArgument="5" OnClick="CompleteAuthentication_Click"
			runat="server" />
		<asp:Button Text="Provider Endpoint (capitalization)" CommandArgument="6" OnClick="CompleteAuthentication_Click"
			runat="server" />
		<asp:Button Text="OpenID Version" CommandArgument="7" OnClick="CompleteAuthentication_Click"
			runat="server" />
		<h4>Tests that should succeed authentication </h4>
		<asp:Button ID="Button1" Text="Claimed Identifier (fragment)" CommandArgument="8"
			OnClick="CompleteAuthentication_Click" runat="server" />
		<asp:Button Text="Claimed Identifier (insignificant query)" CommandArgument="9" OnClick="CompleteAuthentication_Click"
			runat="server" />
	</asp:Panel>
	<h3>Instructions </h3>
	<ol>
		<li>Log into an OpenID 2.0 RP to be tested using this identifier:
			<%= new Uri(Request.Url, Request.Url.AbsolutePath) %>
		</li>
		<li>Upon being redirected to this page for authentication, select a kind of tampering
			technique to apply. </li>
		<li>Record whether the RP rejects the authentication. </li>
		<li>Restart from step 1, until all tampering techniques have been tested. <b>Do not</b>
			simply click the Back button to select another tampering technique. You must reinitiate
			the authentication from the RP to guarantee the RP fails for assertion verification
			reasons rather than request_nonce invalidation. </li>
	</ol>
	<h3>Passing criteria </h3>
	<p>The RP passes if every way to tamper with the assertion generates the expected failed
		or successful authentication at the RP. </p>
</asp:Content>
