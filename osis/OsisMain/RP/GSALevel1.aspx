<%@ Page Title="" Language="C#" MasterPageFile="~/TestMaster.master" AutoEventWireup="true"
	CodeFile="GSALevel1.aspx.cs" Inherits="RP_GSALevel1" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<!-- This identity page doubles as the OP endpoint -->
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/GSALevel1.aspx"
		ProviderVersion="V20" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<p>This is an identity page that implements a GSA level 1 (draft) OpenID Provider.
			</p>
		</asp:View>
		<asp:View ID="RequestNotGsa" runat="server">
			<p>An OpenID authentication request was received, but was not marked as a GSA 
				Level 1 request.&nbsp; This is a failure.</p>
		</asp:View>
		<asp:View ID="RequestGsa" runat="server">
			<p>A GSA Level 1 OpenID authentication request was received. You should complete the
				test by continuing and verify that the RP logs you in. </p>
			<asp:Button runat='server' ID='continueButton' onclick="continueButton_Click" 
				Text="Continue" />
		</asp:View>
	</asp:MultiView>
	<h3>Instructions</h3>
	<ol>
		<li>Log into a GSA Level 1 OpenID RP to be tested using this identifier:
			<%= new Uri(Request.Url, Request.Url.AbsolutePath) %>
		</li>
	</ol>
	<h3>Passing criteria</h3>
	<p>At the conclusion of the steps in the instructions, the RP must recognize that
		<%= new Uri(Request.Url, Request.Url.AbsolutePath)%>
		has logged in. </p>
</asp:Content>
