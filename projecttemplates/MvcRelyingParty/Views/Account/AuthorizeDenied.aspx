<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MvcRelyingParty.Models.AccountAuthorizeModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	AuthorizeDenied
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		AuthorizeDenied
	</h2>
	<p>
		Authorization has been denied. You're free to do whatever now.
	</p>
</asp:Content>
