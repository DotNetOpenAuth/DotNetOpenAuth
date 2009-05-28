<%@ Page Title="" Language="C#" MasterPageFile="~/TestMaster.master" AutoEventWireup="true" CodeFile="POSTAssertion.aspx.cs" Inherits="RP_POSTAssertion" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" Runat="Server">
	<!-- this page does triple-duty: it's a test instruction page, an identity page, and an OP endpoint -->
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/POSTAssertion.aspx" />
	<op:ProviderEndpoint ID="ProviderEndpoint1" runat="server" OnAuthenticationChallenge="ProviderEndpoint1_AuthenticationChallenge" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" Runat="Server">
	<h3>Instructions </h3>
	<ol>
		<li>Log into the RP to be tested using this identifier:
			<%= new Uri(Request.Url, Request.Url.AbsolutePath) %>
		</li>
	</ol>
	<h3>Passing criteria </h3>
	<p>The RP must indicate that login completed successfully. </p>
</asp:Content>

