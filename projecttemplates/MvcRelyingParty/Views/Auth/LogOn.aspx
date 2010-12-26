<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="DotNetOpenAuth.Mvc" %>
<asp:Content ContentPlaceHolderID="Head" runat="server">
	<%= Html.OpenIdSelectorStyles() %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<% Html.RenderPartial("LogOnContent"); %>
</asp:Content>
<asp:Content ContentPlaceHolderID="ScriptsArea" runat="server">
	<% Html.RenderPartial("LogOnScripts"); %>
</asp:Content>