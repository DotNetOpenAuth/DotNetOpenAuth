<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebFormsRelyingParty._Default"
	MasterPageFile="~/Site.Master" %>

<asp:Content runat="server" ContentPlaceHolderID="Body">
	<asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
	<asp:HyperLink ID="HyperLink1" NavigateUrl="~/Members/Default.aspx" Text="Visit the Members Only area"
		runat="server" />
</asp:Content>
