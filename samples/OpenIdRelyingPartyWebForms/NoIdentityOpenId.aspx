<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NoIdentityOpenId.aspx.cs"
	MasterPageFile="~/Site.Master" Inherits="OpenIdRelyingPartyWebForms.NoIdentityOpenId" %>

<asp:Content runat="server" ContentPlaceHolderID="Main">
	<h2>No-login OpenID extension Page </h2>
	<p>This demonstrates an RP sending an extension-only request to an OP that carries
		extensions that request anonymous information about you. In this scenario, the OP
		would still authenticate the user, but would not assert any OpenID Identifier back
		to the RP, but might provide information regarding the user such as age or membership
		in an organization. </p>
	<p><b>Note: </b>At time of this writing, most OPs do not support this feature, although
		it is documented in the OpenID 2.0 spec. </p>
	<asp:Panel runat="server" DefaultButton="beginButton">
		<asp:Label ID="Label1" runat="server" Text="OpenID Identifier" /> <asp:TextBox ID="openIdBox"
			runat="server" />
		<asp:Button ID="beginButton" runat="server" Text="Begin" OnClick="beginButton_Click" />
		<asp:CustomValidator runat="server" ID="openidValidator" ErrorMessage="Invalid OpenID Identifier"
			ControlToValidate="openIdBox" EnableViewState="false" Display="Dynamic" OnServerValidate="openidValidator_ServerValidate" />
		<asp:Label runat="server" EnableViewState="false" ID="resultMessage" />
	</asp:Panel>
	<asp:Panel runat="server" ID="ExtensionResponsesPanel" EnableViewState="false" Visible="false">
		<p>We have received a reasonable response from the Provider. Below is the Simple Registration
			response we received, if any: </p>
		<table id="profileFieldsTable" runat="server">
			<tr>
				<td>Email </td>
				<td><asp:Label runat="server" ID="emailLabel" /> </td>
			</tr>
			<tr>
				<td>Gender </td>
				<td><asp:Label runat="server" ID="genderLabel" /> </td>
			</tr>
			<tr>
				<td>Post Code </td>
				<td><asp:Label runat="server" ID="postalCodeLabel" /> </td>
			</tr>
			<tr>
				<td>Country </td>
				<td><asp:Label runat="server" ID="countryLabel" /> </td>
			</tr>
			<tr>
				<td>Timezone </td>
				<td><asp:Label runat="server" ID="timeZoneLabel" /> </td>
			</tr>
		</table>
	</asp:Panel>
</asp:Content>
