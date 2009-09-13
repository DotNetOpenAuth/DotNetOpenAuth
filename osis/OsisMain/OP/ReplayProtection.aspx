<%@ Page Title="OP provides replay protection" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="ReplayProtection.aspx.cs" Inherits="OP_ReplayProtection" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<rp:OpenIdLogin ID="OpenIdBox" runat="server" ButtonText="Begin" ExamplePrefix=""
				EnableRequestProfile="false" ExampleUrl="" LabelText="OpenID Identifier:" RegisterVisible="False"
				OnLoggedIn="OpenIdBox_LoggedIn" Stateless="true" TabIndex="1" />
		</asp:View>
		<asp:View ID="View2" runat="server">
			<osis:TestResultDisplay runat="server" ID="testResultDisplay" />
		</asp:View>
	</asp:MultiView>
	<h3>
		Instructions
	</h3>
	<ol>
		<li>Enter an OpenID Identifier associated with the Provider to test.</li>
		<li>Complete authentication at the Provider.</li>
	</ol>
	<h3>
		Passing criteria
	</h3>
	<p>
		The Provider must provide successful <a href="http://openid.net/specs/openid-authentication-2_0.html#check_auth" target="_blank">
			direct verification</a> exactly once. A second attempt at direct verification
		will be attempted and that must be denied to protect against replay attacks.
	</p>
</asp:Content>
