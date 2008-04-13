<%@ Page Title="OpenID Relying Party, by DotNetOpenId" Language="C#" MasterPageFile="~/Views/Shared/Site.Master"
	Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
	<h1>Members Only Area </h1>
	<p>Congratulations, <b>
		<asp:LoginName runat="server" />
	</b>. You have completed the OpenID login process. </p>
	<p>
		<%=Html.ActionLink("Logout", "logout") %>
	</p>
</asp:Content>
