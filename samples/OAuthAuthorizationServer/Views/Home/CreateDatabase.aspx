<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Views/Shared/Site.Master" %>

<asp:Content runat="server" ID="Content1" ContentPlaceHolderID="MainContent">
	<% if (ViewData["Success"] != null) {
	%>
	<p>
		Database (re)created!</p>
	<p>
		Note that to be useful, you really need to either modify the database to add an
		account with data that will be accessed by this sample, or modify this very page
		to inject that data into the database.
	</p>
	<%
		}
	%>
	<p style="color: Red; font-weight: bold">
		<%= ViewData["Error"] %>
	</p>
</asp:Content>
