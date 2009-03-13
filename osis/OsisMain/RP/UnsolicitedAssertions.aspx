<%@ Page Title="RP accepts unsolicited assertions" Language="C#" MasterPageFile="~/TestMaster.master"
	AutoEventWireup="true" CodeFile="UnsolicitedAssertions.aspx.cs" Inherits="RP_UnsolicitedAssertions" %>

<asp:Content ID="Content2" ContentPlaceHolderID="TestBody" runat="Server">
	<table>
		<tr>
			<td>RP Realm (usually the home page) </td>
			<td><asp:TextBox ID="rpRealmBox" Columns="40" runat="server" />
				<asp:RequiredFieldValidator ControlToValidate="rpRealmBox" ID="RequiredFieldValidator1"
					runat="server" ErrorMessage="required" />
			</td>
		</tr>
		<%--		<tr>
			<td>RP return_to </td>
			<td><asp:TextBox ID="rpReturnToBox" runat="server" /> </td>
		</tr>
--%>
	</table>
	<asp:Button ID="beginButton" runat="server" Text="Send unsolicited assertion" OnClick="beginButton_Click" />
	<h3>Instructions </h3>
	<ol>
		<li>Ensure that you are logged out of the RP to be tested.</li>
		<li>Provide a verifiable RP realm URL.</li>
		<li>Click "Send unsolicited assertion".</li>
		<li>After recording whether the RP logged you in automatically, use the Back button
			on your browser to return here. </li>
	</ol>
	<h3>Passing criteria </h3>
	<p>After sending the unsolicited assertion, you should see the RP site, and without
		any further login process you should be logged in as
		<%= new Uri(Request.Url, Page.ResolveUrl("AffirmativeIdentity.aspx")) %>. </p>
</asp:Content>
