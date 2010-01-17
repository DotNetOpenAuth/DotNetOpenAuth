<%@ Page Language="VB" AutoEventWireup="true" MasterPageFile="~/Site.Master" %>
<%@ Import Namespace="OpenIdRelyingPartyWebFormsVB" %>
<%@ Register Src="~/MembersOnly/ProfileFieldsDisplay.ascx" TagPrefix="cc1" TagName="ProfileFieldsDisplay" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
	<h2>
		Members Only Area
	</h2>
	<p>
		Congratulations, <b><asp:LoginName ID="LoginName1" runat="server" /></b>. 
		You have completed the OpenID login process.
	</p>

<% 	If (State.PapePolicies IsNot Nothing) Then%>
	<p>A PAPE extension was included in the authentication with this content: </p>
	<ul>
	<% 	If (State.PapePolicies.NistAssuranceLevel.HasValue) Then%>
		<li>Nist: <%=HttpUtility.HtmlEncode(State.PapePolicies.NistAssuranceLevel.Value.ToString())%></li>
	<% End If
		For Each policy As String In State.PapePolicies.ActualPolicies%>
		<li><%=HttpUtility.HtmlEncode(policy)%></li>
	<% 	Next%>
	</ul>
<% End If %>

<% 	If State.ProfileFields IsNot Nothing Then
		profileFieldsDisplay.ProfileValues = State.ProfileFields%>
	<p>
		In addition to authenticating you, your OpenID Provider may
		have told us something about you using the 
		Simple Registration extension:
	</p>
	<cc1:ProfileFieldsDisplay runat="server" ID="profileFieldsDisplay" />
<% 	End If%>
</asp:Content>
