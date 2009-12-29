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
