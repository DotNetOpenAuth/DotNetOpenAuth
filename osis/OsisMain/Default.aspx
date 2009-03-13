<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPage.master" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth" TagPrefix="openid" %>
<asp:Content runat="server" ContentPlaceHolderID="head">
	<openid:XrdsPublisher ID="XrdsPublisher1" runat="server" XrdsUrl="~/OP/ReturnToVerification.Xrds.aspx"
		XrdsAdvertisement="Both" />
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="Body">
	<p>This is the OSIS I5 OpenID interop testing site. </p>
	<asp:TreeView ID="TreeView1" runat="server" DataSourceID="siteMapDataSource" />
	<asp:SiteMapDataSource runat="server" ID="siteMapDataSource" ShowStartingNode="false" />
</asp:Content>
