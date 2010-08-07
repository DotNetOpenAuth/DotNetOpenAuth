<%@ Page Language="C#" AutoEventWireup="true" Inherits="OpenIdProviderWebForms.login" CodeBehind="login.aspx.cs" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
	<p>
		Usernames are defined in the App_Data\Users.xml file.
	</p>
	<asp:Login runat="server" ID="login1" />
	
	<p>Credentials to try (each with their own OpenID)</p>
	<table>
		<tr><td>Username</td><td>Password</td></tr>
		<tr><td>bob</td><td>test</td></tr>
		<tr><td>bob1</td><td>test</td></tr>
		<tr><td>bob2</td><td>test</td></tr>
		<tr><td>bob3</td><td>test</td></tr>
		<tr><td>bob4</td><td>test</td></tr>
	</table>

	<asp:Panel DefaultButton="yubicoButton" runat="server" style="margin-top: 25px" ID="yubicoPanel">
		Login with Yubikey: 
		<asp:TextBox runat="server" type="text" ID="yubicoBox" ToolTip="Click here and press your Yubikey button."
			style="background-image: url(http://yubico.com/favicon.ico); background-repeat: no-repeat; background-position: 0px 1px; padding-left: 18px; width: 20em;" 
			MaxLength="44" AutoCompleteType="Disabled" />
		<asp:Button runat="server" ID="yubicoButton" Text="Login" 
			onclick="yubicoButton_Click" />
		<asp:Label Text="[Yubikey Result]" runat="server" EnableViewState="false" Visible="false" ForeColor="Red" ID="yubikeyFailureLabel" />
	</asp:Panel>
</asp:Content>