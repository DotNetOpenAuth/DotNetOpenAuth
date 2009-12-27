<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%
If Session("ClaimedIdentifier") = "" Then
	Response.Redirect("login.asp")
End If
%>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>DotNetOpenAuth Classic ASP sample: Members Only area</title>
	<link href="styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
	<div>
		<a href="http://dotnetopenauth.net">
			<img runat="server" src="images/DotNetOpenAuth.png" title="Jump to the project web site."
				alt="DotNetOpenAuth" border='0' /></a>
	</div>
	<h2>
		Members Only Area
	</h2>
	<p>
		Congratulations, <b><%=Session("ClaimedIdentifier") %></b>. 
		You have completed the OpenID login process.
	</p>
	<p>Additional data we may have about you using the Simple Registration extension:</p>
	<table>
		<tr><td>Email </td><td><%=Session("Email") %> </td></tr>
		<tr><td>Nickname </td><td><%=Session("Nickname") %> </td></tr>
		<tr><td>Full name </td><td><%=Session("FullName") %> </td></tr>
	</table>
	<p><a href="logout.asp">Log out</a>. </p>
</body>
</html>
