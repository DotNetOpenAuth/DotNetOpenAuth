<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebFormsRelyingParty._Default"
	MasterPageFile="~/Site.Master" Title="OpenID + InfoCard Relying Party template" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.UI" Namespace="DotNetOpenAuth" TagPrefix="dnoa" %>
<asp:Content runat="server" ContentPlaceHolderID="head">
	<dnoa:XrdsPublisher runat="server" XrdsUrl="~/xrds.aspx" XrdsAdvertisement="Both" />
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="Body">
	<asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
	<asp:HyperLink ID="HyperLink1" NavigateUrl="~/Members/Default.aspx" Text="Visit the Members Only area"
		runat="server" />
</asp:Content>
