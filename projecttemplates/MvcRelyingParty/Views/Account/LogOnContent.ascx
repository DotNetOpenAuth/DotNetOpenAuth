<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="DotNetOpenAuth.Mvc" %>
<p>Login using an account you already use. </p>
<%= Html.ValidationSummary("Login was unsuccessful. Please correct the errors and try again.") %>

<% using (Html.BeginForm("LogOnPostAssertion", "Account", FormMethod.Post, new { target = "_top" })) { %>
<%= Html.AntiForgeryToken() %>
<%= Html.Hidden("ReturnUrl", Request.QueryString["ReturnUrl"], new { id = "ReturnUrl" }) %>
<%= Html.Hidden("openid_openidAuthData") %>
<div>
	<ul class="OpenIdProviders">
		<%= Html.OpenIdSelectorOPButton(this.Page, "https://me.yahoo.com/", Url.Content("~/Content/images/yahoo.gif"))%>
		<%= Html.OpenIdSelectorOPButton(this.Page, "https://www.google.com/accounts/o8/id", Url.Content("~/Content/images/google.gif")) %>
		<%= Html.OpenIdSelectorOpenIdButton(this.Page, Url.Content("~/Content/images/openid.gif")) %>
	</ul>
	<%= Html.OpenIdAjaxTextBox() %>

	<div class="helpDoc">
		<p>
			If you have logged in previously, click the same button you did last time.
		</p>
		<p>
			If you don't have an account with any of these services, just pick Google. They'll
			help you set up an account.
		</p>
	</div>

</div>
<% } %>
