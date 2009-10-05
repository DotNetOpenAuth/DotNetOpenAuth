<%@ Page Title="RP ignores Content-Location header in response from claimed identifier"
	Language="C#" MasterPageFile="~/TestMaster.master" AutoEventWireup="true" CodeFile="IgnoresContentLocationHeader.aspx.cs"
	Inherits="RP_IgnoresContentLocationHeader" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="TestHead">
	<!-- This identity page doubles as the OP endpoint -->
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/IgnoresContentLocationHeader.aspx"
		ProviderVersion="V20" />
	<op:ProviderEndpoint ID="ProviderEndpoint1" runat='server' OnAuthenticationChallenge="ProviderEndpoint1_AuthenticationChallenge" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView runat="server" ID="multiView1" ActiveViewIndex="0">
		<asp:View runat="server">
		</asp:View>
		<asp:View ID="testResultsView" runat="server">
			<osis:TestResultDisplay runat="server" ID="testResultDisplay1" />
			<asp:Panel runat="server" Visible="false" ID="warningPanel">
				<p>
					Continue to follow through and see if this bug is an exploitable security hole.
					It's vulnerable if the Continue button successfully logs you in as
					<asp:Label runat="server" ID="victimIdLabel" />
				</p>
				<asp:Button runat="server" OnClick="continueLoginButton_Click" Text="Continue" />
			</asp:Panel>
		</asp:View>
	</asp:MultiView>
	<h3>
		Instructions
	</h3>
	<ol>
		<li>Log into the RP under test with the identifier:
			<%= new Uri(Request.Url, Request.Url.AbsolutePath) %>
		</li>
	</ol>
</asp:Content>
