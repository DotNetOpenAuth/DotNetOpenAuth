<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPage.master" %>

<asp:Content runat="server" ContentPlaceHolderID="Body">
	<p>This is the OSIS I5 OpenID interop testing site. </p>
	<asp:TreeView ID="TreeView1" runat="server" DataSourceID="siteMapDataSource" />
	<asp:SiteMapDataSource runat="server" ID="siteMapDataSource" ShowStartingNode="false" />
</asp:Content>
