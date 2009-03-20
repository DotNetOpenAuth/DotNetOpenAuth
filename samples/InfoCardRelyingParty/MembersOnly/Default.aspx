<%@ Page Language="VB" AutoEventWireup="true" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
	<h2>
		Members Only Area
	</h2>
	<p>
		Congratulations, <b><%= Session("SiteSpecificID") %></b>. 
		You have completed the InfoCard login process.
	</p>
	<p>Your secure unique ID on this site is <asp:LoginName ID="LoginName1" runat="server" />.</p>
</asp:Content>
