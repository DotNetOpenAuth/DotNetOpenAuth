<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebFormsRelyingParty.Admin.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
</head>
<body>
	<form id="form1" runat="server">
	<div>
		This is the specially-privileged Admin area. You can only reach here if you are
		a member of the Admin role.
	</div>
	<asp:Repeater runat="server" ID="usersRepeater">
		<HeaderTemplate>
			<table border="1">
				<thead>
					<tr>
						<td>
							Email
						</td>
						<td>
							Claimed IDs
						</td>
					</tr>
				</thead>
				<tbody>
		</HeaderTemplate>
		<ItemTemplate>
			<tr>
				<td>
					<%# HttpUtility.HtmlEncode((string)Eval("EmailAddress")) %>
				</td>
				<td>
					<asp:Repeater runat="server" DataSource='<%# Eval("AuthenticationTokens") %>'>
						<ItemTemplate>
							<%# HttpUtility.HtmlEncode((string)Eval("ClaimedIdentifier")) %>
						</ItemTemplate>
						<SeparatorTemplate>
							<br />
						</SeparatorTemplate>
					</asp:Repeater>
				</td>
			</tr>
		</ItemTemplate>
		<FooterTemplate>
			</tbody> </table>
		</FooterTemplate>
	</asp:Repeater>
	</form>
</body>
</html>
