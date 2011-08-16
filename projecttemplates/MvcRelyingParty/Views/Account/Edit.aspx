<%@ Page Title="Edit Account Information" Language="C#" MasterPageFile="~/Views/Shared/Site.Master"
	Inherits="System.Web.Mvc.ViewPage<AccountInfoModel>" %>

<%@ Import Namespace="MvcRelyingParty.Models" %>
<%@ Import Namespace="DotNetOpenAuth.Mvc" %>
<%@ Import Namespace="DotNetOpenAuth.OpenId.RelyingParty" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Edit
</asp:Content>
<asp:Content ContentPlaceHolderID="Head" runat="server">
	<%= Html.OpenIdSelectorStyles() %>
</asp:Content>
<asp:Content ContentPlaceHolderID="ScriptsArea" runat="server">
	<script type="text/javascript" src='<%= Url.Content("~/Scripts/MicrosoftAjax.js") %>'></script>
	<script type="text/javascript" src='<%= Url.Content("~/Scripts/MicrosoftMvcAjax.js") %>'></script>
	<script type="text/javascript" src='<%= Url.Content("~/Scripts/jquery.cookie.js") %>'></script>
	<% var selector = new OpenIdSelector();
	selector.TextBox.LogOnText = "ADD";
	selector.TextBox.LogOnToolTip = "Bind this OpenID to your account.";
	var additionalOptions = new OpenIdAjaxOptions {
		FormIndex = 1,
	}; %>
	<%= Html.OpenIdSelectorScripts(selector, additionalOptions)%>
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
