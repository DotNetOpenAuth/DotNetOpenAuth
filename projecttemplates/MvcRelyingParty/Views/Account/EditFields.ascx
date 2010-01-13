<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<MvcRelyingParty.Models.AccountInfoModel>" %>
<%= Html.ValidationSummary("Edit was unsuccessful. Please correct the errors and try again.") %>
<fieldset>
	<legend>Account Details</legend>
	<table>
		<tr>
			<td>
				<label for="FirstName">First Name:</label>
			</td>
			<td>
				<%= Html.TextBox("FirstName", Model.FirstName) %>
				<%= Html.ValidationMessage("FirstName", "*") %>
			</td>
		</tr>
		<tr>
			<td>
				<label for="LastName">Last Name:</label>
			</td>
			<td>
				<%= Html.TextBox("LastName", Model.LastName) %>
				<%= Html.ValidationMessage("LastName", "*") %>
			</td>
		</tr>
		<tr>
			<td>
				<label for="EmailAddress">Email Address:</label>
			</td>
			<td>
				<%= Html.TextBox("EmailAddress", Model.EmailAddress) %>
				<%= Html.ValidationMessage("EmailAddress", "*") %>
			</td>
		</tr>
	</table>
</fieldset>
