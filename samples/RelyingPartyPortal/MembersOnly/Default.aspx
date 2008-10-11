<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
	<h2>
		Members Only Area
	</h2>
	<p>
		Congratulations, <b><asp:LoginName ID="LoginName1" runat="server" /></b>. 
		You have completed the OpenID login process.
	</p>

<% if (State.PapePolicies != null) { %>
	<p>A PAPE extension was included in the authentication with this content: </p>
	<ul>
	<% if (State.PapePolicies.NistAssuranceLevel != null) {%>
		<li>Nist: <%=HttpUtility.HtmlEncode(State.PapePolicies.NistAssuranceLevel.Value.ToString())%></li>
	<% }
	foreach (string policy in State.PapePolicies.ActualPolicies) { %>
		<li><%=HttpUtility.HtmlEncode(policy) %></li>
	<% } %>
	</ul>
<% } %>

<% if (State.ProfileFields != null) { %>
	<p>
		In addition to authenticating you, your OpenID Provider may
		have told us something about you using the 
		Simple Registration extension:
	</p>
	<table id="profileFieldsTable" runat="server">
		<tr>
			<td>
				Nickname
			</td>
			<td>
				<%=State.ProfileFields.Nickname %>
			</td>
		</tr>
		<tr>
			<td>
				Email
			</td>
			<td>
				<%=State.ProfileFields.Email%>
			</td>
		</tr>
		<tr>
			<td>
				FullName
			</td>
			<td>
				<%=State.ProfileFields.FullName%>
			</td>
		</tr>
		<tr>
			<td>
				Date of Birth
			</td>
			<td>
				<%=State.ProfileFields.BirthDate.ToString()%>
			</td>
		</tr>
		<tr>
			<td>
				Gender
			</td>
			<td>
				<%=State.ProfileFields.Gender.ToString()%>
			</td>
		</tr>
		<tr>
			<td>
				Post Code
			</td>
			<td>
				<%=State.ProfileFields.PostalCode%>
			</td>
		</tr>
		<tr>
			<td>
				Country
			</td>
			<td>
				<%=State.ProfileFields.Country%>
			</td>
		</tr>
		<tr>
			<td>
				Language
			</td>
			<td>
				<%=State.ProfileFields.Language%>
			</td>
		</tr>
		<tr>
			<td>
				Timezone
			</td>
			<td>
				<%=State.ProfileFields.TimeZone%>
			</td>
		</tr>
	</table>
<% } %>
</asp:Content>
