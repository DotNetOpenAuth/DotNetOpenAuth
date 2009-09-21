<%@ Page Title="RP accepts POST assertion with UTF-8 multi-byte characters" Language="C#"
	MasterPageFile="~/TestMaster.master" AutoEventWireup="true" CodeFile="POSTAssertionWithUtf8.aspx.cs"
	Inherits="RP_POSTAssertionWithUtf8" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<!-- this page does triple-duty: it's a test instruction page, an identity page, and an OP endpoint -->
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/POSTAssertionWithUtf8.aspx" />
	<op:ProviderEndpoint ID="ProviderEndpoint1" runat="server" OnAuthenticationChallenge="ProviderEndpoint1_AuthenticationChallenge" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView runat="server" ID="multiView" ActiveViewIndex='0'>
		<asp:View runat="server">
			<h3>
				Instructions
			</h3>
			<ol>
				<li>Log into the RP to be tested using this identifier:
					<%= new Uri(Request.Url, Request.Url.AbsolutePath) %>
				</li>
			</ol>
			<h3>
				Passing criteria
			</h3>
			<p>
				The RP must indicate that login completed successfully.
			</p>
		</asp:View>
		<asp:View runat="server" ID="sharedAssociationRequired">
			Test indefinitive because the relying party sent the request without a shared association
			handle.
		</asp:View>
	</asp:MultiView>
</asp:Content>
