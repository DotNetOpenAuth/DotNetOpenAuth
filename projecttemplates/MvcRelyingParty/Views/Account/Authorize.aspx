<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MvcRelyingParty.Models.AccountAuthorizeModel>" %>

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
		<%= Html.Encode(Model.ConsumerApp) %>
		application is requesting to access the private data in your account here. Is that
		alright with you?
	</p>
	<p>
		If you grant access now, you can revoke it at any time by returning to
		<%= Html.ActionLink("your account page", "Edit") %>.
	</p>
	<% using (Html.BeginForm()) { %>
		<%= Html.AntiForgeryToken() %>
		<%= Html.Hidden("IsApproved") %>
		<div style="display: none" id="responseButtonsDiv">
			<input type="submit" value="Yes" onclick="document.getElementsByName("IsApproved")[0].value = true; return true;" />
			<input type="submit" value="No" onclick="document.getElementsByName("IsApproved")[0].value = false; return true;" />
		</div>
		<div id="javascriptDisabled">
			<b>Javascript appears to be disabled in your browser. </b>This page requires Javascript
			to be enabled to better protect your security.
		</div>
		<% if (Model.IsUnsafeRequest) { %>
		<div style="background-color: red; color: white; font-weight: bold">
			This website is registered with
			<asp:Label runat="server" ID="serviceProviderDomainNameLabel" />
			to make authorization requests, but has not been configured to send requests securely.
			If you grant access but you did not initiate this request at
			<%= Html.Encode(Model.ConsumerApp) %>, it may be possible for other users of
			<%= Html.Encode(Model.ConsumerApp) %>
			to access your data. We recommend you deny access unless you are certain that you
			initiated this request directly with
			<%= Html.Encode(Model.ConsumerApp) %>.
		<% } %>

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
