<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="loginGoogleApps.aspx.cs"
	Inherits="OpenIdRelyingPartyWebForms.loginGoogleApps" ValidateRequest="false"
	MasterPageFile="~/Site.Master" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.RelyingParty.UI" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
	<rp:OpenIdLogin ID="OpenIdLogin1" runat="server" ExampleUrl="yourname@yourdomain.com"
		TabIndex="1" LabelText="Google Apps email address or domain:" 
		RegisterVisible="False" onloggedin="OpenIdLogin1_LoggedIn" />
	<asp:Panel runat="server" ID="fullTrustRequired" EnableViewState="false">
		<b>STOP:</b> Full trust permissions are required for Google Apps support
		due to certificate chain verification requirements.
		Modify web.config to allow full trust before trying this sample.
	</asp:Panel>
</asp:Content>
