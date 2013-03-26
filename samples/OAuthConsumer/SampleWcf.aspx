<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" Inherits="OAuthConsumer.SampleWcf" Codebehind="SampleWcf.aspx.cs" Async="true" %>

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
	<br />
	<asp:Button ID="getNameButton" runat="server" Text="Get Name" OnClick="getNameButton_Click" />
	<asp:Label ID="nameLabel" runat="server" />
	<br />
	<asp:Button ID="getAgeButton" runat="server" Text="Get Age" OnClick="getAgeButton_Click" />
	<asp:Label ID="ageLabel" runat="server" />
	<br />
	<asp:Button ID="getFavoriteSites" runat="server" Text="Get Favorite Sites" 
		onclick="getFavoriteSites_Click" />
	<asp:Label ID="favoriteSitesLabel" runat="server" />
</asp:Content>
