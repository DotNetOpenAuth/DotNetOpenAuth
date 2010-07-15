<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<MvcRelyingParty.Models.AccountInfoModel>" %>
<h3>
	Authorized applications
</h3>
<% if (Model.AuthorizedApps.Count == 0) { %>
<p>
	You have not authorized any applications or web sites to access your data.
</p>
<% } else { %>
	<ul>
	<% foreach (var app in Model.AuthorizedApps) { %>
		<li><%= Html.Encode(app.AppName) %> - <%= Html.Encode(app.Scope) %> - <%= Ajax.ActionLink("revoke", "RevokeAuthorization", new { authorizationId = app.AuthorizationId }, new AjaxOptions { HttpMethod = "DELETE", UpdateTargetId = "authorizedApps", OnFailure = "function(e) { alert('Revoking authorization for this application failed.'); }" })%></li>
	<% } %>
	</ul>
<% } %>