<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Welcome OpenID User!</title>
</head>
<body>
	<form id="form1" runat="server">
	<h1>
		Members Only Area
	</h1>
	<p>
		Congratulations, <b>
			<asp:LoginName ID="LoginName1" runat="server" />
		</b>. You have completed the OpenID login process.
	</p>
	<asp:LoginStatus ID="LoginStatus1" runat="server" />

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
	</form>
</body>
</html>
