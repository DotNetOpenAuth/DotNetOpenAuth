<%@ Page Title="RP validates the 1.1 signature" Language="C#" MasterPageFile="~/TestMaster.master" AutoEventWireup="true"
	CodeFile="SignatureCheck11.aspx.cs" Inherits="RP_SignatureCheck11" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<!-- this page does triple-duty: it's a test instruction page, an identity page, and an OP endpoint -->
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/SignatureCheck11.aspx"
		ProviderVersion="V11" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:Panel ID="AuthPanel" runat="server" EnableViewState="false" Visible="false">
		<p>The RP has requested authentication. Please choose a way to meddle with the asserted
			info.</p>
		<h4>Tests that should fail authentication </h4>
		<asp:Button Text="Invalid signature (shared)" CommandArgument="1" OnClick="CompleteAuthentication_Click" runat="server" ID="invalidSignatureSharedButton" />
		<asp:Button Text="Invalid signature (private)" CommandArgument="2" OnClick="CompleteAuthentication_Click" runat="server" />
		<asp:Button Text="Signature missing return_to" CommandArgument="3" OnClick="CompleteAuthentication_Click" runat="server" />
		<asp:Button Text="Signature missing identity" CommandArgument="4" OnClick="CompleteAuthentication_Click" runat="server" />
	</asp:Panel>
	<h3>Instructions </h3>
	<ol>
		<li>Log into an OpenID RP to be tested using this identifier:
			<%= new Uri(Request.Url, Request.Url.AbsolutePath) %>
		</li>
		<li>Upon being redirected to this page for authentication, select a kind of tampering
			technique to apply. </li>
		<li>Record whether the RP rejects the authentication. </li>
		<li>Restart from step 1, until all tampering techniques have been tested. <b>Do not</b>
			simply click the Back button to select another tampering technique. You must reinitiate
			the authentication from the RP to guarantee the RP fails for verification
			reasons rather than request_nonce invalidation. </li>
	</ol>
	<h3>Passing criteria </h3>
	<p>The RP passes if every way to tamper with the assertion generates the expected failed
		or successful authentication at the RP. </p>
</asp:Content>
