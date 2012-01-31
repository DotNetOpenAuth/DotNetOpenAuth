<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Default.aspx.vb" Inherits="OpenIdRelyingPartyWebFormsVB._Default"
	MasterPageFile="~/Site.Master" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.UI" Namespace="DotNetOpenAuth" TagPrefix="openid" %>
<asp:Content runat="server" ContentPlaceHolderID="head">
	<openid:XrdsPublisher ID="XrdsPublisher1" runat="server" XrdsUrl="~/xrds.aspx" />
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="main">
	<h2>
		Relying Party
	</h2>
	<p>
		Visit the
		<asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/MembersOnly/Default.aspx"
			Text="Members Only" />
		area. (This will trigger a login demo).
	</p>
</asp:Content>
