<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	OpenID Provider endpoint
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h2>OpenID Provider endpoint</h2>
	<p>This page expects to receive OpenID authentication messages to allow users to log
		into other web sites. </p>
</asp:Content>
