<%@ Page Title="RP has discoverable return_to" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="DiscoverableReturnTo.aspx.cs" Inherits="RP_DiscoverableReturnTo" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<!-- this page does triple-duty: it's a test instruction page, an identity page, and an OP endpoint -->
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/DiscoverableReturnTo.aspx" />
	<op:ProviderEndpoint ID="ProviderEndpoint1" runat="server" OnAuthenticationChallenge="ProviderEndpoint1_AuthenticationChallenge" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:Panel ID="resultsPanel" runat="server" Visible="false">
		<p>Incoming authentication request from: <asp:Label ID="realmLabel" runat="server" />
		</p>
		<asp:MultiView ID="MultiView1" runat="server">
			<asp:View ID="PassView" runat="server">
				<span style="font-weight: bold; color: Green">PASS</span>: RP discovery at the realm URL turned up a matching return_to URL.
			</asp:View>
			<asp:View ID="FailNoXrds" runat="server">
				<span style="font-weight: bold; color: Red">FAIL</span>: RP discovery failed to find an XRDS document.
			</asp:View>
			<asp:View ID="FailNoMatchingReturnTo" runat="server">
				<span style="font-weight: bold; color: Red">FAIL</span>: RP discovery found an XRDS document that did not have a matching return_to URI.
			</asp:View>
		</asp:MultiView>
	</asp:Panel>
	<h3>Instructions </h3>
	<ol>
		<li>Log into the RP to be tested using this identifier:
			<%= new Uri(Request.Url, Request.Url.AbsolutePath) %>
		</li>
	</ol>
</asp:Content>
