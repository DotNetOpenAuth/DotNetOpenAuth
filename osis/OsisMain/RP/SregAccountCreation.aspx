<%@ Page Title="RP account creation using Simple Registration" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="SregAccountCreation.aspx.cs" Inherits="RP_SregAccountCreation" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<%@ Register Src="ProfileFields.ascx" TagName="ProfileFields" TagPrefix="uc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestHead" runat="Server">
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/SregAccountCreation.aspx"
		ProviderVersion="V20" />
	<op:ProviderEndpoint ID="ProviderEndpoint1" runat='server' OnAuthenticationChallenge="ProviderEndpoint1_AuthenticationChallenge" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server">
		<asp:View ID="NoSregRequest" runat="server">
			<p>The RP under test sent an authentication request without including the Simple Registration
				extension. The RP has FAILED this test. </p>
		</asp:View>
		<asp:View ID="SregRequestDetails" runat="server">
			<p>An authentication request carrying a Simple Registration extension has been received.
				You should continue to complete authentication and the test. </p>
			<p>
				<asp:Button runat="server" ID="continueButton" Text="Continue" OnClick="continueButton_Click" />
			</p>
			<uc1:ProfileFields runat="server" ID="profileFields" />
		</asp:View>
	</asp:MultiView>
	<h3>Instructions </h3>
	<ol>
		<li>Log into an RP using the Claimed Identifier: <asp:Label runat="server" Font-Bold="true"
			ID="claimedIdLabel" /></li>
	</ol>
	<h3>Passing criteria </h3>
	<p>The RP passes if authentication completes and the RP account creation process has
		been streamlined using profile values provided by this test, allowing the tester
		to avoid having to provide personal data such as full name, email address, etc.
	</p>
</asp:Content>
