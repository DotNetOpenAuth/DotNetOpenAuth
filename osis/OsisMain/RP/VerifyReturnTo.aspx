<%@ Page Title="RP verifies return_to URL" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="VerifyReturnTo.aspx.cs" Inherits="RP_VerifyReturnTo" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<!-- this page does triple-duty: it's a test instruction page, an identity page, and an OP endpoint -->
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/VerifyReturnTo.aspx" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:Panel ID="AuthPanel" runat="server" EnableViewState="false" Visible="false">
		<p>The RP has requested authentication. Please choose a way to meddle with the return_to
			URL.</p>
		<asp:Button ID="Button1" OnClick="CompleteAuthentication_Click" CommandArgument="1"
			runat="server" Text="Scheme" />
		<asp:Button ID="Button2" OnClick="CompleteAuthentication_Click" CommandArgument="2"
			runat="server" Text="Host name" />
		<asp:Button ID="Button3" OnClick="CompleteAuthentication_Click" CommandArgument="3"
			runat="server" Text="Port" />
		<asp:Button ID="Button4" OnClick="CompleteAuthentication_Click" CommandArgument="4"
			runat="server" Text="Path (significant)" />
		<asp:Button ID="Button5" OnClick="CompleteAuthentication_Click" CommandArgument="5"
			runat="server" Text="Path (capitalization only)" />
		<asp:Button ID="Button6" OnClick="CompleteAuthentication_Click" CommandArgument="6"
			runat="server" Text="Extra query parameter" />
	</asp:Panel>
	<h3>Instructions </h3>
	<ol>
		<li>Log into the RP to be tested using this identifier:
			<%= new Uri(Request.Url, Request.Url.AbsolutePath) %>
		</li>
		<li>Upon being redirected to this page for authentication, select a kind of return_to
			tampering technique to apply. </li>
		<li>Record whether the RP rejects the authentication. </li>
		<li>Restart from step 1, until all return_to tampering techniques have been tested.
			<b>Do not</b> simply click the Back button to select another tampering technique.
			You must reinitiate the authentication from the RP to guarantee the RP fails for
			return_to verification reasons rather than request_nonce invalidation. </li>
	</ol>
	<h3>Passing criteria </h3>
	<p>The RP passes if every way to tamper with the return_to URL generates a failed authentication
		at the RP. </p>
</asp:Content>
