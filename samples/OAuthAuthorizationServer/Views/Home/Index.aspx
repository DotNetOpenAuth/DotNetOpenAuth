<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Home Page
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		DotNetOpenAuth presents the OAuth 2.0 Authorization Server!
	</h2>
	<p>
		To learn more about DotNetOpenAuth visit <a href="http://www.DotNetOpenAuth.net/"
			title="DotNetOpenAuth web site">http://www.DotNetOpenAuth.net/</a>.
	</p>
	<% using (Html.BeginForm("CreateDatabase", "Home")) {%>
	<input type="submit" value="(Re)Create Database" />
	<%
		}%>
</asp:Content>
