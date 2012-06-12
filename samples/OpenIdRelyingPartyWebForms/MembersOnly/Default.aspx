<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" %>
<%@ Import Namespace="OpenIdRelyingPartyWebForms" %>
<%@ Register Src="~/MembersOnly/ProfileFieldsDisplay.ascx" TagPrefix="cc1" TagName="ProfileFieldsDisplay" %>
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
	<% }
	if (State.PapePolicies.AuthenticationTimeUtc.HasValue) { %>		
		<li>The provider authenticated the user at <%=State.PapePolicies.AuthenticationTimeUtc.Value.ToLocalTime() %> (local time)</li>
	<% } %>
	</ul>
<% } %>

<% if (State.ProfileFields != null) {
		 profileFieldsDisplay.ProfileValues = State.ProfileFields; %>
	<p>
		In addition to authenticating you, your OpenID Provider may
		have told us something about you using the 
		Simple Registration extension:
	</p>
	<cc1:ProfileFieldsDisplay runat="server" ID="profileFieldsDisplay" />
<% } %>
</asp:Content>
