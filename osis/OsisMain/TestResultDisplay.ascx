<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TestResultDisplay.ascx.cs"
	Inherits="TestResultDisplay" %>
<div style="width: 700px; background-color: yellow; margin: auto;">
	<table class="thnowrap">
		<tr>
			<th>Provider endpoint: </th>
			<td><asp:Label ID="endpointLabel" runat="server" /></td>
		</tr>
		<tr>
			<th>OpenID version: </th>
			<td><asp:Label ID="protocolVersion" runat="server" /></td>
		</tr>
		<tr>
			<th>Result: </th>
			<td><asp:Label ID="passLabel" Text="PASS" Font-Bold="true" runat="server" ForeColor="Green" Visible="false" /> <asp:Label
				ID="failLabel" Text="FAIL" Font-Bold="true" runat="server" ForeColor="Red" Visible="false" /> </td>
		</tr>
		<tr>
			<th>Details: </th>
			<td>
				<asp:Label ID="detailsLabel" runat="server" />
			</td>
		</tr>
	</table>
</div>
