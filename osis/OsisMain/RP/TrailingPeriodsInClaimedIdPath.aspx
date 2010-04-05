<%@ Page Language="VB" AutoEventWireup="false" CodeFile="TrailingPeriodsInClaimedIdPath.aspx.vb"
	Inherits="RP_TrailingPeriodsInClaimedIdPath" MasterPageFile="~/MasterPage.master"
	Title="RP correctly handles paths with trailing periods" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content runat="server" ContentPlaceHolderID="head">
	<op:IdentityEndpoint ID="IdentityEndpoint1" runat="server" ProviderEndpointUrl="~/RP/TrailingPeriodsInClaimedIdPath.aspx"
		ProviderVersion="V11" Enabled="false" />
	<op:IdentityEndpoint ID="IdentityEndpoint2" runat="server" ProviderEndpointUrl="~/RP/TrailingPeriodsInClaimedIdPath.aspx"
		ProviderVersion="V20" Enabled="false" />
	<op:ProviderEndpoint ID="ProviderEndpoint1" runat="server" OnAuthenticationChallenge="ProviderEndpoint1_AuthenticationChallenge" />
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="Body">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<h3>
				Instructions
			</h3>
			<ol>
				<li>Visit the RP and log in with this OpenID Identifier:
					<%= New Uri(Request.Url, Request.Url.AbsolutePath).AbsoluteUri + "/a."%>
				</li>
			</ol>
			<h3>
				Passing criteria
			</h3>
			<p>
				The RP passes this test if it successfully logs in the user as:
				<%= New Uri(Request.Url, Request.Url.AbsolutePath).AbsoluteUri + "/a."%>
			</p>
		</asp:View>
		<asp:View runat="server" ID="ResultsView">
			<osis:TestResultDisplay runat="server" ID="testResultDisplay" />
		</asp:View>
	</asp:MultiView>
</asp:Content>
