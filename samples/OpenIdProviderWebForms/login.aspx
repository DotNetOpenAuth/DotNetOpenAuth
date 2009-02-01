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
</asp:Content>