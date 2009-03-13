<%@ Page Title="OP performs multi-factor authentication" Language="C#" MasterPageFile="~/TestMaster.master" AutoEventWireup="true"
	CodeFile="MultiFactor.aspx.cs" Inherits="OP_MultiFactor" %>

<%@ Register Src="~/TestResultDisplay.ascx" TagPrefix="osis" TagName="TestResultDisplay" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TestBody" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<rp:OpenIdLogin ID="OpenIdBox" runat="server" OnLoggedIn="OpenIdBox_LoggedIn" OnLoggingIn="OpenIdBox_LoggingIn"
				ButtonText="Begin" ExamplePrefix="" ExampleUrl="" LabelText="OpenID Identifier:"
				RegisterVisible="False" TabIndex="1" />
		</asp:View>
		<asp:View ID="View2" runat="server">
			<osis:TestResultDisplay runat="server" ID="testResultDisplay" />
		</asp:View>
	</asp:MultiView>
	<h3>
		Instructions:
	</h3>
	<ol>
		<li>Log out of your Provider if you are logged in. </li>
		<li>Enter your OpenID Identifier above and click Begin. </li>
		<li>Record whether your Provider required at least two credentials to authenticate you.
			For example, a password plus a text message on your cell phone. </li>
	</ol>
	<h3>
		Passing criteria:
	</h3>
	<p>A Provider passes this test if it required two factor authentication in order to
		complete authentication to this page and it correctly reports this back. </p>
</asp:Content>
