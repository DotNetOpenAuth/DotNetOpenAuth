<%@ Page Title="Edit Account Information" Language="C#" MasterPageFile="~/Views/Shared/Site.Master"
	Inherits="System.Web.Mvc.ViewPage<AccountInfoModel>" %>

<%@ Import Namespace="MvcRelyingParty.Models" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Edit
</asp:Content>
<asp:Content ContentPlaceHolderID="ScriptsArea" runat="server">
	<script src="../../Scripts/MicrosoftAjax.js" type="text/javascript"></script>
	<script src="../../Scripts/MicrosoftMvcAjax.js" type="text/javascript"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		Edit Account Information
	</h2>
	<% using (Ajax.BeginForm("Update", new AjaxOptions { HttpMethod = "PUT", UpdateTargetId = "postResult" })) { %>
	<%= Html.AntiForgeryToken()%>
	<table>
		<tr>
			<td>
				First name
			</td>
			<td>
				<input name="firstName" value="<%= Html.Encode(Model.FirstName) %>" />
			</td>
		</tr>
		<tr>
			<td>
				Last name
			</td>
			<td>
				<input name="lastName" value="<%= Html.Encode(Model.LastName) %>" />
			</td>
		</tr>
		<tr>
			<td>
				Email
			</td>
			<td>
				<input name="emailAddress" value="<%= Html.Encode(Model.EmailAddress) %>" />
			</td>
		</tr>
	</table>
	<div>
		<input type="submit" value="Save" />
		<div id="postResult" />
	</div>
	<% } %>
</asp:Content>
