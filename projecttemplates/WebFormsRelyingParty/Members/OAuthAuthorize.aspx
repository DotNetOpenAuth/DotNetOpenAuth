<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
	CodeBehind="OAuthAuthorize.aspx.cs" Inherits="WebFormsRelyingParty.Members.OAuthAuthorize" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="server">
	<h2>
		Client authorization</h2>
	<p>
		The
		<asp:Label ID="consumerNameLabel" runat="server" Text="(app name)" />
		application is requesting to access the private data in your account here. Is that
		alright with you?
	</p>
	<asp:Button ID="yesButton" runat="server" Text="Yes" 
		onclick="yesButton_Click" />
	&nbsp;<asp:Button ID="noButton" runat="server" Text="No" 
		onclick="noButton_Click" />
	<asp:HiddenField runat="server" ID="csrfCheck" EnableViewState="false" />
</asp:Content>
