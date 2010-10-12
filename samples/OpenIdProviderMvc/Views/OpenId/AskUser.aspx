<%@ Page Title="Do you want to log into another web site?" Language="C#" MasterPageFile="~/Views/Shared/Site.Master"
	Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		Logging in somewhere?
	</h2>
	<p>
		Are you trying to log into
		<b><%= Html.Encode(ViewData["Realm"]) %></b>?
	</p>
	<% using (Html.BeginForm("AskUserResponse", "OpenId")) { %>
	<%= Html.AntiForgeryToken() %>
	<%= Html.Hidden("confirmed", "false") %>
	<div style="display: none" id="responseButtonsDiv">
		<input type="submit" value="yes" onclick="document.getElementsByName('confirmed')[0].value = 'true'; return true;" />
		<input type="submit" value="no" />
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
