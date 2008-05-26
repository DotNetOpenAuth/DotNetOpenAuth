<%@ Page Language="C#" AutoEventWireup="true" Inherits="decide" CodeBehind="decide.aspx.cs" %>

<%@ Register Src="ProfileFields.ascx" TagName="ProfileFields" TagPrefix="uc1" %>
<html>
<head>
	<title>Approve OpenID request?</title>
</head>
<body>
	<form id="Form1" runat="server">
	<p>
		A site has asked to authenticate that you own the identifier below.&nbsp; You should 
		only do this if you wish to log in to the site given by the Realm.</p>
	<p>
		This site
		<asp:Label ID="relyingPartyVerificationResultLabel" runat="server" 
			Font-Bold="True" Text="failed" /> verification. </p>
	<table>
		<tr>
			<td>
				Identifier: 			</td>
			<td>
				<asp:Label runat="server" ID='identityUrlLabel' />
			</td>
		</tr>
		<tr>
			<td>
				Realm: 			</td>
			<td>
				<asp:Label runat="server" ID='realmLabel' />
			</td>
		</tr>
	</table>
	<p>
		Allow this authentication to proceed?
	</p>
	<uc1:ProfileFields ID="profileFields" runat="server" Visible="false" />
	<asp:Button ID="yes_button" OnClick="Yes_Click" Text="  yes  " runat="Server" />
	<asp:Button ID="no_button" OnClick="No_Click" Text="  no  " runat="Server" />
	</form>
</body>
</html>
