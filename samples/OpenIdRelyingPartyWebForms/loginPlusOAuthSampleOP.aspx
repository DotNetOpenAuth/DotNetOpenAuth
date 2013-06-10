<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="loginPlusOAuthSampleOP.aspx.cs"
	Inherits="OpenIdRelyingPartyWebForms.loginPlusOAuthSampleOP" ValidateRequest="false"
	MasterPageFile="~/Site.Master" Async="true" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.RelyingParty.UI" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
	<h2>Login Page </h2>
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex='0'>
		<asp:View ID="View1" runat="server">
			<asp:Label runat="server" Text="OpenIdProviderWebForms sample's OP Identifier or Claimed Identifier: " />
			<rp:OpenIdTextBox runat="server" ID="identifierBox" Text="http://localhost:4860/"
				OnLoggingIn="identifierBox_LoggingIn" OnLoggedIn="identifierBox_LoggedIn" OnCanceled="identifierBox_Failed"
				OnFailed="identifierBox_Failed" />
			<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="required"
				ControlToValidate="identifierBox" />
			<br />
			<asp:Button ID="beginButton" runat="server" Text="Login + OAuth request" OnClick="beginButton_Click" />
		</asp:View>
		<asp:View ID="AuthorizationGiven" runat="server">
			Authentication succeeded, and OAuth access was granted.
			<p>The actual login step is aborted since this sample focuses on the process only
				up to this point.</p>
		</asp:View>
		<asp:View ID="AuthorizationDenied" runat="server">
			Authentication succeeded, but OAuth access was denied.
			<p>The actual login step is aborted since this sample focuses on the process only
				up to this point.</p>
		</asp:View>
		<asp:View ID="AuthenticationFailed" runat="server">
			Authentication failed or was canceled.
		</asp:View>
	</asp:MultiView>
</asp:Content>
