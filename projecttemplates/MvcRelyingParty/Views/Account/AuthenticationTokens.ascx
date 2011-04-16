<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<MvcRelyingParty.Models.AccountInfoModel>" %>
<%@ Import Namespace="DotNetOpenAuth.Mvc" %>
<%@ Import Namespace="DotNetOpenAuth.OpenId.RelyingParty" %>

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

<% using(Html.BeginForm("AddAuthenticationToken", "Auth", FormMethod.Post)) { %>
<%= Html.AntiForgeryToken() %>
<%= Html.Hidden("openid_openidAuthData") %>

<%= Html.OpenIdSelector(new SelectorButton[] {
	new SelectorProviderButton("https://me.yahoo.com/", Url.Content("~/Content/images/yahoo.gif")),
	new SelectorProviderButton("https://www.google.com/accounts/o8/id", Url.Content("~/Content/images/google.gif")),
	new SelectorOpenIdButton(Url.Content("~/Content/images/openid.gif")),
}) %>

<% } %>