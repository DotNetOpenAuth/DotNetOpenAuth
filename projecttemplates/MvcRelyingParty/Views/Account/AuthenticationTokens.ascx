<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<MvcRelyingParty.Models.AccountInfoModel>" %>
<h3>
	Login methods
</h3>
<ul class="AuthTokens">
<% foreach(var token in Model.AuthenticationTokens) { %>
	<li class="<%= token.IsInfoCard ? "InfoCard" : "OpenID" %>" title="<%= Html.Encode(token.ClaimedIdentifier) %>">
		<%= Html.Encode(token.FriendlyIdentifier) %>
	</li>
<% } %>
</ul>

<h4>Add a new login method </h4>
<% using(Html.BeginForm("AddAuthenticationToken", "Account", FormMethod.Post)) { %>
	<%= Html.AntiForgeryToken() %>
	<label for="openid_identifier">OpenID:</label>
	<%= Html.TextBox("openid_identifier")%>
	<%= Html.ValidationMessage("openid_identifier")%>
	<input type="submit" value="Add token" />
<% } %>