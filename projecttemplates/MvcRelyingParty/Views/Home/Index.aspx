<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="indexTitle" ContentPlaceHolderID="TitleContent" runat="server">
	Home Page
</asp:Content>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		<%= Html.Encode(ViewData["Message"]) %></h2>
	<p>
		To learn more about DotNetOpenAuth visit <a href="http://www.dotnetopenauth.net/"
			title="DotNetOpenAuth web site">http://www.dotnetopenauth.net/</a>.
	</p>
	<p>
		Try logging in.
	</p>
</asp:Content>
