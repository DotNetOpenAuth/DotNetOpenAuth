<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MvcRelyingParty.Models.AccountAuthorizeModel>" %>
<%@ Import Namespace="DotNetOpenAuth.OAuth2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Authorize
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		Authorize
	</h2>
	<div style="background-color: Yellow">
		<b>Warning</b>: Never give your login credentials to another web site or application.
	</div>
	<p>
		The
		<%= Html.Encode(Model.ClientApp) %>
		application is requesting to access the private data in your account here. Is that
		alright with you?
	</p>
	<p>
		<b>Requested access: </b>
		<%= Html.Encode(String.Join(" ", Model.Scope.ToArray())) %>
	</p>
	<p>
		If you grant access now, you can revoke it at any time by returning to
		<%= Html.ActionLink("your account page", "Edit") %>.
	</p>
	<% using (Html.BeginForm("AuthorizeResponse", "Account")) { %>
		<%= Html.AntiForgeryToken() %>
		<%= Html.Hidden("IsApproved") %>
		<%= Html.Hidden("client_id", Model.AuthorizationRequest.ClientIdentifier) %>
		<%= Html.Hidden("redirect_uri", Model.AuthorizationRequest.Callback) %>
		<%= Html.Hidden("state", Model.AuthorizationRequest.ClientState) %>
		<%= Html.Hidden("scope", OAuthUtilities.JoinScopes(Model.AuthorizationRequest.Scope)) %>
		<%= Html.Hidden("response_type", "code") %>
		<div style="display: none" id="responseButtonsDiv">
			<input type="submit" value="Yes" onclick="document.getElementsByName('IsApproved')[0].value = true; return true;" />
			<input type="submit" value="No" onclick="document.getElementsByName('IsApproved')[0].value = false; return true;" />
		</div>
		<div id="javascriptDisabled">
			<b>Javascript appears to be disabled in your browser. </b>This page requires Javascript
			to be enabled to better protect your security.
		</div>

		<script language="javascript" type="text/javascript">
			//<![CDATA[
			// we use HTML to hide the action buttons and Javascript to show them
			// to protect against click-jacking in an iframe whose javascript is disabled.
			document.getElementById('responseButtonsDiv').style.display = 'block';
			document.getElementById('javascriptDisabled').style.display = 'none';

			// Frame busting code (to protect us from being hosted in an iframe).
			// This protects us from click-jacking.
			if (document.location !== window.top.location) {
				window.top.location = document.location;
			}
			//]]>
		</script>

	<% } %>
</asp:Content>
