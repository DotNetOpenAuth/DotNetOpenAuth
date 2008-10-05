<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
	CodeFile="SampleWcf.aspx.cs" Inherits="SampleWcf" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<asp:Button ID="getAuthorizationButton" runat="server" Text="Get Authorization" OnClick="getAuthorizationButton_Click" />
	<asp:Label ID="authorizationLabel" runat="server" />
	<br />
	<asp:Button ID="getNameButton" runat="server" Text="Get Name" OnClick="getNameButton_Click" />
	<asp:Label ID="nameLabel" runat="server" />
	<br />
	<asp:Button ID="getAgeButton" runat="server" Text="Get Age" OnClick="getAgeButton_Click" />
	<asp:Label ID="ageLabel" runat="server" />
</asp:Content>
