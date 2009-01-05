<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="login.aspx.cs" Inherits="login"
	ValidateRequest="false" MasterPageFile="~/Site.Master" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.RelyingParty" TagPrefix="cc1" %>
<asp:Content runat="server" ContentPlaceHolderID="Main">
	<h2>Login Page </h2>
	<cc1:OpenIdLogin ID="OpenIdLogin1" runat="server" CssClass="openid_login" RequestCountry="Request"
		RequestEmail="Request" RequestGender="Require" RequestPostalCode="Require" RequestTimeZone="Require"
		RememberMeVisible="True" PolicyUrl="~/PrivacyPolicy.aspx" TabIndex="1"
		OnLoggedIn="OpenIdLogin1_LoggedIn" OnLoggingIn="OpenIdLogin1_LoggingIn"
		OnCanceled="OpenIdLogin1_Canceled" OnFailed="OpenIdLogin1_Failed" OnSetupRequired="OpenIdLogin1_SetupRequired" />
	<fieldset title="Knobs">
		<asp:CheckBox ID="requireSslCheckBox" runat="server" 
			Text="RequireSsl (high security) mode" 
			oncheckedchanged="requireSslCheckBox_CheckedChanged" /><br />
		<asp:CheckBox ID="immediateCheckBox" runat="server" Text="Immediate mode" /><br />
		<asp:CheckBoxList runat="server" ID="papePolicies">
			<asp:ListItem Text="Request phishing resistant authentication" Value="http://schemas.openid.net/pape/policies/2007/06/phishing-resistant" />
			<asp:ListItem Text="Request multi-factor authentication" Value="http://schemas.openid.net/pape/policies/2007/06/multi-factor" />
			<asp:ListItem Text="Request physical multi-factor authentication" Value="http://schemas.openid.net/pape/policies/2007/06/multi-factor-physical" />
		</asp:CheckBoxList>
	</fieldset>
	<br />
	<asp:Label ID="loginFailedLabel" runat="server" EnableViewState="False" Text="Login failed"
		Visible="False" />
	<asp:Label ID="loginCanceledLabel" runat="server" EnableViewState="False" Text="Login canceled"
		Visible="False" />
	<p>
		<asp:ImageButton runat="server" ImageUrl="~/images/yahoo.png" ID="yahooLoginButton"
			OnClick="yahooLoginButton_Click" />
	</p>
</asp:Content>
