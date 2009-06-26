<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="login.aspx.cs" Inherits="OpenIdRelyingPartyWebForms.login"
	ValidateRequest="false" MasterPageFile="~/Site.Master" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty" TagPrefix="rp" %>
<asp:Content runat="server" ContentPlaceHolderID="Main">
	<h2>Login Page </h2>
	<rp:OpenIdLogin ID="OpenIdLogin1" runat="server" CssClass="openid_login" RequestCountry="Request"
		RequestEmail="Require" RequestGender="Require" RequestPostalCode="Require" RequestTimeZone="Require"
		RememberMeVisible="True" PolicyUrl="~/PrivacyPolicy.aspx" TabIndex="1"
		OnLoggedIn="OpenIdLogin1_LoggedIn" OnLoggingIn="OpenIdLogin1_LoggingIn"
		OnSetupRequired="OpenIdLogin1_SetupRequired" />
	<fieldset title="Knobs">
		<asp:CheckBox ID="requireSslCheckBox" runat="server" 
			Text="RequireSsl (high security) mode" 
			oncheckedchanged="requireSslCheckBox_CheckedChanged" /><br />
		<h4 style="margin-top: 0; margin-bottom: 0">PAPE policies</h4>
		<asp:CheckBoxList runat="server" ID="papePolicies">
			<asp:ListItem Text="Request phishing resistant authentication" Value="http://schemas.openid.net/pape/policies/2007/06/phishing-resistant" />
			<asp:ListItem Text="Request multi-factor authentication" Value="http://schemas.openid.net/pape/policies/2007/06/multi-factor" />
			<asp:ListItem Text="Request physical multi-factor authentication" Value="http://schemas.openid.net/pape/policies/2007/06/multi-factor-physical" />
			<asp:ListItem Text="Request PPID identifier" Value="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier" />
		</asp:CheckBoxList>
		<p>Try the PPID identifier functionality against the OpenIDProviderMvc sample.</p>
	</fieldset>
	<br />
	<asp:Label ID="setupRequiredLabel" runat="server" EnableViewState="False" Text="You must log into your Provider first to use Immediate mode."
		Visible="False" />
	<p>
		<rp:OpenIdButton runat="server" ImageUrl="~/images/yahoo.png" Text="Login with Yahoo!" ID="yahooLoginButton"
			Identifier="https://me.yahoo.com/" />
	</p>
</asp:Content>
