<%@ Page Title="RP checks response_nonce for replays" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="ResponseNonceCheck.aspx.cs" Inherits="RP_ResponseNonceCheck" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<!-- this page does triple-duty: it's a test instruction page, an identity page, and an OP endpoint -->
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/ResponseNonceCheck.aspx"
		ProviderVersion="V20" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:Panel ID="AuthPanel" runat="server" EnableViewState="false" Visible="false">
		<p>The RP has requested authentication. Click Log In.</p>
		<asp:Button ID="loginButton" runat="server" Text="Log In" OnClick="loginButton_Click" />
	</asp:Panel>
	<asp:Panel ID="DumbModeInvalidPanel" runat="server" EnableViewState="false" Visible="false">
		<p>The RP has requested authentication. </p>
		<p><b>Test invalid:</b> The RP is operating in 'dumb' mode. Shared associations are
			required for a valid RP response_nonce test. </p>
	</asp:Panel>
	<h3>Instructions </h3>
	<ol>
		<li>Log into an OpenID 2.0 RP to be tested using this identifier:
			<%= new Uri(Request.Url, Request.Url.AbsolutePath) %>
		</li>
		<li>Upon being redirected to this page for authentication, click the Log In button.
		</li>
		<li>Verify that the RP accepted the authentication.</li>
		<li>Click your browser's Back button to get back here. Do <b>not</b> click Refresh.</li>
		<li>Click the Log In button again.</li>
		<li>Record whether the RP rejects the authentication. </li>
	</ol>
	<h3>Passing criteria </h3>
	<p>The RP passes if it accepts the first Log In attempt and rejects the second.
	</p>
</asp:Content>
