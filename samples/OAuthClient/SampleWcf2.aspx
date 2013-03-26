<%@ Page Title="OAuth 2.0 client (web server flow)" Language="C#" MasterPageFile="~/MasterPage.master" Async="true"
	AutoEventWireup="true" Inherits="OAuthClient.SampleWcf2" CodeBehind="SampleWcf2.aspx.cs" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<fieldset title="Authorization">
		<p>
			Check off the operations you&#39;d like to authorization this client to make on
			behalf of your account on the resource server.<br />
			Note that an authorization request may not actually result in you being prompted
			to grant authorization if you&#39;ve granted it previously.&nbsp; The authorization
			server remembers what you&#39;ve already approved.&nbsp; But even if you&#39;ve
			requested and received authorization for all three scopes above, you can request
			access tokens for subsets of this set of scopes to limit what you can do below.
		</p>
		<asp:CheckBoxList runat="server" ID="scopeList">
			<asp:ListItem Value="http://tempuri.org/IDataApi/GetName">GetName</asp:ListItem>
			<asp:ListItem Value="http://tempuri.org/IDataApi/GetAge">GetAge</asp:ListItem>
			<asp:ListItem Value="http://tempuri.org/IDataApi/GetFavoriteSites">GetFavoriteSites</asp:ListItem>
		</asp:CheckBoxList>
		<asp:Button ID="getAuthorizationButton" runat="server" Text="Request Authorization"
			OnClick="getAuthorizationButton_Click" />
		<asp:Label ID="authorizationLabel" runat="server" />
	</fieldset>
	<br />
	<asp:Button ID="getNameButton" runat="server" Text="Get Name" OnClick="getNameButton_Click" />
	<asp:Label ID="nameLabel" runat="server" />
	<br />
	<asp:Button ID="getAgeButton" runat="server" Text="Get Age" OnClick="getAgeButton_Click" />
	<asp:Label ID="ageLabel" runat="server" />
	<br />
	<asp:Button ID="getFavoriteSites" runat="server" Text="Get Favorite Sites" OnClick="getFavoriteSites_Click" />
	<asp:Label ID="favoriteSitesLabel" runat="server" />
</asp:Content>
