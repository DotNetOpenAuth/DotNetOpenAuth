<%@ Page Title="RP protects against association poisoning" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="AssociationPoisoning.aspx.cs" Inherits="RP_AssociationPoisoning" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<op:IdentityEndpoint runat="server" ID="IdentityTest1" Visible="false" ProviderEndpointUrl="http://test-id.org/rp/AssociationPoisoningOP.aspx" />
	<op:IdentityEndpoint runat="server" ID="IdentityTest2" Visible="false" ProviderEndpointUrl="http://www.test-id.org/rp/AssociationPoisoningOP.aspx" />
	<op:IdentityEndpoint runat="server" ID="IdentityTest3" Visible="false" ProviderEndpointUrl="https://test-id.org/rp/AssociationPoisoningOP.aspx" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:Panel runat="server" ID="StatelessRPInvalid" EnableViewState="false" Visible="false">
		<p>The test is invalid for this RP because it is operating in dumb/stateless mode. </p>
	</asp:Panel>
	<h3>Instructions </h3>
	<ol>
		<li>Log into the RP under test with identifier
			<%= new Uri(Request.Url, Request.Url.AbsolutePath).AbsoluteUri + "?test=1" %>
		</li>
		<li>Log into the RP under test with identifier
			<%= new Uri(Request.Url, Request.Url.AbsolutePath).AbsoluteUri + "?test=2" %>
		</li>
		<li>Log into the RP under test with identifier
			<%= new Uri(Request.Url, Request.Url.AbsolutePath).AbsoluteUri + "?test=3" %>
		</li>
	</ol>
	<h3>Passing criteria </h3>
	<p>The RP under test passes if the second and third login attempts FAIL, possibly mentioning
		an invalid signature. </p>
</asp:Content>
