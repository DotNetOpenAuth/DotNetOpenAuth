<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" Inherits="OAuthConsumer.SampleWcf2" Codebehind="SampleWcf2.aspx.cs" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<fieldset title="Authorization">
		<asp:CheckBoxList runat="server" ID="scopeList">
			<asp:ListItem Value="http://tempuri.org/IDataApi/GetName">GetName</asp:ListItem>
			<asp:ListItem Value="http://tempuri.org/IDataApi/GetAge">GetAge</asp:ListItem>
			<asp:ListItem Value="http://tempuri.org/IDataApi/GetFavoriteSites">GetFavoriteSites</asp:ListItem>
		</asp:CheckBoxList>
		<asp:Button ID="getAuthorizationButton" runat="server" Text="Get Authorization" OnClick="getAuthorizationButton_Click" />
		<asp:Label ID="authorizationLabel" runat="server" />
	</fieldset>
</asp:Content>