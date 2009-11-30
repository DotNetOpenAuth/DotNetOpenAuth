<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="loginGoogleApps.aspx.cs"
	Inherits="OpenIdRelyingPartyWebForms.loginGoogleApps" ValidateRequest="false"
	MasterPageFile="~/Site.Master" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
	<rp:OpenIdLogin ID="OpenIdLogin1" runat="server" ExampleUrl="yourname@yourdomain.com"
		TabIndex="1" LabelText="Google Apps email address or domain:" 
		RegisterVisible="False" onloggedin="OpenIdLogin1_LoggedIn" />
	<p>
		<b>NOTE:</b> Full trust permissions are required. Modify web.config to allow full
		trust before trying this sample.
	</p>
</asp:Content>
