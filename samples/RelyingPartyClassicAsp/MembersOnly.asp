<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%
If Session("ClaimedIdentifier") = "" Then
	Response.Redirect("login.asp")
End If
%>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>DotNetOpenId Classic ASP sample: Members Only area</title>
	<link href="styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
	<div>
		<a href="http://dotnetopenid.googlecode.com">
			<img runat="server" src="images/dotnetopenid_tiny.gif" title="Jump to the project web site."
				alt="DotNetOpenId" border='0' /></a>
	</div>
	<h2>
		Members Only Area
	</h2>
	<p>
		Congratulations, <b><%=Session("ClaimedIdentifier") %></b>. 
		You have completed the OpenID login process.
	</p>
	<p><a href="logout.asp">Log out</a>. </p>
</body>
</html>
