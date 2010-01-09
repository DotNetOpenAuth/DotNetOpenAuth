<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="loginTitle" ContentPlaceHolderID="TitleContent" runat="server">
	Log On
</asp:Content>
<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		Log On
	</h2>
	<%= Html.ValidationSummary("Login was unsuccessful. Please correct the errors and try again.") %>

	<% using (Html.BeginForm("LogOn", "Account")) { %>
	<%= Html.AntiForgeryToken() %>
	<%= Html.Hidden("ReturnUrl", Request.QueryString["ReturnUrl"]) %>
	<div>
		<fieldset>
			<legend>Account Information</legend>
			<p>
				<label for="openid_identifier">OpenID:</label>
				<%= Html.TextBox("openid_identifier")%>
				<%= Html.ValidationMessage("openid_identifier")%>
			</p>
			<p>
				<%= Html.CheckBox("rememberMe") %> <label class="inline" for="rememberMe">Remember me?</label>
			</p>
			<p>
				<input type="submit" value="Log On" />
			</p>
		</fieldset>
	</div>
	<% } %>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ScriptsArea" runat="server">
	<script type="text/javascript" src="../../Scripts/MicrosoftAjax.js"></script>
	<script type="text/javascript" src="../../Scripts/MicrosoftMvcAjax.js"></script>
	<script type="text/javascript" language="javascript"><!--//<![CDATA[
	$addHandler(window, 'load', function() { document.getElementsByName("openid_identifier")[0].focus(); });
	//]]>--></script>
</asp:Content>
