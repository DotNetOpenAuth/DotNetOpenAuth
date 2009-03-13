<%@ Page Title="OpenID Provider support for Attribute Exchange" Language="C#" MasterPageFile="~/OP/ProviderTests.master"
	AutoEventWireup="true" CodeFile="AXFetch.aspx.cs" Inherits="OP_AXFetch" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<rp:OpenIdLogin ID="OpenIdBox" runat="server" ButtonText="Begin" ExamplePrefix=""
				ExampleUrl="" LabelText="OpenID Identifier:" RegisterVisible="False" TabIndex="1" />
			<asp:Table ID="FetchRequestTable" runat="server">
				<asp:TableRow runat="server">
					<asp:TableCell runat="server">Description</asp:TableCell>
					<asp:TableCell runat="server">Type URI</asp:TableCell>
					<asp:TableCell runat="server">Count</asp:TableCell>
					<asp:TableCell runat="server">Required</asp:TableCell>
				</asp:TableRow>
			</asp:Table>
		</asp:View>
		<asp:View ID="View2" runat="server">
			AX fetch response:
			<asp:Table runat="server" ID="FetchResponseTable">
				<asp:TableRow>
					<asp:TableCell>Description</asp:TableCell>
					<asp:TableCell>Value</asp:TableCell>
				</asp:TableRow>
			</asp:Table>
		</asp:View>
	</asp:MultiView>
	<h3>
		Instructions
	</h3>
	<ol>
		<li>Customize the AX fetch request if desired, including entering a custom AX attribute
			if you wish.</li>
		<li>Enter an OpenID Identifier associated with the Provider to test.</li>
		<li>Complete authentication at the Provider. Note whether the Provider asks for permission
			to send personal information with the authentication.</li>
		<li>Check that the attribute values show up back here.</li>
	</ol>
	<h3>
		Passing criteria
	</h3>
	<p>The Provider must recognize the standard attribute Type URIs above and successfully
		transfer the values to this page. </p>
</asp:Content>
