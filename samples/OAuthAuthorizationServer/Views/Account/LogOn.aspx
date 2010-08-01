<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<OAuthAuthorizationServer.Models.LogOnModel>" %>

<asp:Content ID="loginTitle" ContentPlaceHolderID="TitleContent" runat="server">
	Log On
</asp:Content>
<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		Log On</h2>
	<% using (Html.BeginForm()) { %>
	<%: Html.ValidationSummary(true, "Login was unsuccessful. Please correct the errors and try again.") %>
	<div>
		<fieldset>
			<legend>Account Information</legend>
			<div class="editor-label">
				<%: Html.LabelFor(m => m.UserSuppliedIdentifier) %>
			</div>
			<div class="editor-field">
				<%: Html.TextBoxFor(m => m.UserSuppliedIdentifier) %>
				<%: Html.ValidationMessageFor(m => m.UserSuppliedIdentifier) %>
			</div>
			<div class="editor-label">
				<%: Html.CheckBoxFor(m => m.RememberMe) %>
				<%: Html.LabelFor(m => m.RememberMe) %>
			</div>
			<p>
				<input type="submit" value="Log On" />
			</p>
		</fieldset>
	</div>
	<% } %>
</asp:Content>
