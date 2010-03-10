<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="RelyingPartyLogic" %>
<%@ Import Namespace="System.Linq" %>
<%
	if (Request.IsAuthenticated) {
%>
Welcome <b>
	<%
		var authToken = Database.DataContext.AuthenticationTokens.Include("User").First(token => token.ClaimedIdentifier == Page.User.Identity.Name);
		if (!string.IsNullOrEmpty(authToken.User.EmailAddress)) {
			Response.Write(HttpUtility.HtmlEncode(authToken.User.EmailAddress));
		} else if (!string.IsNullOrEmpty(authToken.User.FirstName)) {
			Response.Write(HttpUtility.HtmlEncode(authToken.User.FirstName));
		} else {
			Response.Write(HttpUtility.HtmlEncode(authToken.FriendlyIdentifier));
		}
	%>
</b>! [
<%= Html.ActionLink("Log Off", "LogOff", "Auth") %>
]
<%
	} else {
%>
[
<a href="#" class="loginPopupLink">Login / Register</a>
]
<%
	}
%>
