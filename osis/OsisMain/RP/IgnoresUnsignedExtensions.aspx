<%@ Page Title="RP ignores unsigned extensions" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="IgnoresUnsignedExtensions.aspx.cs" Inherits="RP_IgnoresUnsignedExtensions" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<!-- this page does triple-duty: it's a test instruction page, an identity page, and an OP endpoint -->
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/IgnoresUnsignedExtensions.aspx"
		ProviderVersion="V20" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:Panel runat="server" Visible="false" EnableViewState="false" ID="MissingOrUnsupportedExtensionRequest">
		<p>
			<b>Limitations:</b> This test currently only supports responding to RPs with unsigned
			AX extensions that request email address and names. No supported extension request
			was detected.
		</p>
	</asp:Panel>
	<h3>
		Instructions
	</h3>
	<ol>
		<li>Log into an OpenID RP to be tested using this identifier:
			<%= new Uri(Request.Url, Request.Url.AbsolutePath) %>
		</li>
		<li>Record whether the RP logs the user in and leverages the extension response.
		</li>
	</ol>
	<h3>
		Passing criteria
	</h3>
	<p>
		The RP passes if it ignores all extension responses.
	</p>
</asp:Content>
