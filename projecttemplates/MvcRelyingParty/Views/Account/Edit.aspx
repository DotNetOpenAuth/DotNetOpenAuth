<%@ Page Title="Edit Account Information" Language="C#" MasterPageFile="~/Views/Shared/Site.Master"
	Inherits="System.Web.Mvc.ViewPage<AccountInfoModel>" %>

<%@ Import Namespace="MvcRelyingParty.Models" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Edit
</asp:Content>
<asp:Content ContentPlaceHolderID="ScriptsArea" runat="server">

	<script src="../../Scripts/MicrosoftAjax.js" type="text/javascript"></script>

	<script src="../../Scripts/MicrosoftMvcAjax.js" type="text/javascript"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		Edit Account Information
	</h2>
	<% using (Ajax.BeginForm("Update", new AjaxOptions { HttpMethod = "PUT", UpdateTargetId = "editPartial", LoadingElementId = "updatingMessage" })) { %>
	<%= Html.AntiForgeryToken()%>
	<div id="editPartial">
		<% Html.RenderPartial("EditFields"); %>
	</div>
	<input type="submit" value="Save" />
	<span id="updatingMessage" style="display: none">Saving...</span>
	<% } %>

	<div id="authorizedApps">
		<% Html.RenderPartial("AuthorizedApps"); %>
	</div>

	<div id="authenticationTokens">
		<% Html.RenderPartial("AuthenticationTokens"); %>
	</div>
</asp:Content>
