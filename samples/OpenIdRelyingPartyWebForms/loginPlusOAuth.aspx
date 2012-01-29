<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="loginPlusOAuth.aspx.cs"
	Inherits="OpenIdRelyingPartyWebForms.loginPlusOAuth" ValidateRequest="false"
	MasterPageFile="~/Site.Master" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.RelyingParty.UI" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
	<h2>Login Page </h2>
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex='0'>
		<asp:View ID="View1" runat="server">
			<p><b>Important note:</b> Do <b>not</b> check the &quot;Allow [this site] to remember me&quot; 
				check box while Google is asking for verification. Doing so will make this 
				sample only work once for your account. If you do check it, you can restore this 
				sample&#39;s functionality by <a href="https://www.google.com/accounts/IssuedAuthSubTokens">
					revoking access</a> to this site from your Google Account. </p>
			<asp:Button ID="beginButton" runat="server" Text="Login and get Gmail Contacts" OnClick="beginButton_Click" />
			<p>Due to the way Google matches realms and consumer keys, this demo will only work
				when it is run under http://demo.dotnetopenauth.net/. By registering your own consumer
				key with Google and changing the configuration of this sample, you can run it on
				your own public web site, but it can never work from a private (localhost or firewall-protected)
				address. </p>
		</asp:View>
		<asp:View ID="AuthorizationDenied" runat="server">
			Authentication succeeded, but Gmail Contacts access was denied.
		</asp:View>
		<asp:View ID="AuthenticationFailed" runat="server">
			Authentication failed or was canceled.
		</asp:View>
	</asp:MultiView>
</asp:Content>
