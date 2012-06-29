<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="login.aspx.cs" Inherits="OpenIdRelyingPartyWebForms.login"
	ValidateRequest="false" MasterPageFile="~/Site.Master" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.RelyingParty.UI" Namespace="DotNetOpenAuth.OpenId.RelyingParty" TagPrefix="rp" %>
<%@ Register Assembly="DotNetOpenAuth.OpenId" Namespace="DotNetOpenAuth.OpenId.Extensions.SimpleRegistration" TagPrefix="sreg" %>
<asp:Content runat="server" ContentPlaceHolderID="Main">
	<h2>Login Page </h2>
	<rp:OpenIdLogin ID="OpenIdLogin1" runat="server" CssClass="openid_login" RequestCountry="Request"
		RequestEmail="Require" RequestGender="Require" RequestPostalCode="Require" RequestTimeZone="Require"
		RememberMeVisible="True" PolicyUrl="~/PrivacyPolicy.aspx" TabIndex="1"
		OnLoggedIn="OpenIdLogin1_LoggedIn" OnLoggingIn="OpenIdLogin1_LoggingIn" />
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
		<p>Request that the Provider have authenticated the user in the last 
			<asp:TextBox runat="server" ID="maxAuthTimeBox" MaxLength="4" Columns="4" />
			seconds.
		</p>
		<p>Try the PPID identifier functionality against the OpenIDProviderMvc sample.</p>
	</fieldset>
	<p><a href="loginGoogleApps.aspx">Log in using Google Apps for Domains</a>. </p>
	<p>
		<rp:OpenIdButton runat="server" ImageUrl="~/images/yahoo.png" Text="Login with Yahoo!" ID="yahooLoginButton"
			Identifier="https://me.yahoo.com/" OnLoggingIn="OpenIdLogin1_LoggingIn" OnLoggedIn="OpenIdLogin1_LoggedIn">
			<Extensions>
				<sreg:ClaimsRequest Email="Require" />
			</Extensions>
		</rp:OpenIdButton>
	</p>
</asp:Content>
