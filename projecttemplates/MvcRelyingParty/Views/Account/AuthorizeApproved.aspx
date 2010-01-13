<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MvcRelyingParty.Models.AccountAuthorizeModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Authorized
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		Authorized
	</h2>
	<p>
		Authorization has been granted.
	</p>
	<% if (!string.IsNullOrEmpty(Model.VerificationCode)) { %>
	<p>
		You must enter this verification code at the Consumer: <b>
			<%= Html.Encode(Model.VerificationCode)%>
		</b>
	</p>
	<% } else { %>
	<p>
		You may now close this window and return to the Consumer.
	</p>
	<% } %>
</asp:Content>
