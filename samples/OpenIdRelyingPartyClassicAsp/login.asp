<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>DotNetOpenAuth Classic ASP sample: Login</title>
	<link href="styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
	<div>
		<a href="http://dotnetopenauth.net">
			<img runat="server" src="images/DotNetOpenAuth.png" title="Jump to the project web site."
				alt="DotNetOpenAuth" border='0' /></a>
	</div>
	<h2>Login Page</h2>
	<%
	dim realm, thisPageUrl, requestUrl, dnoi, authentication
	realm = "http://" + Request.ServerVariables("HTTP_HOST") + "/classicaspdnoi/" ' change this to be the home page of your web site, without the filename.
	requestUrl = "http://" + Request.ServerVariables("HTTP_HOST") + Request.ServerVariables("HTTP_URL") ' this is the full URL of the current incoming request.
	Set dnoi = server.CreateObject("DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingParty")
	On Error Resume Next
	' Since this page both starts the OpenID authentication flow and receives the response, we don't
	' yet know whether this particular request is already in the response phase.  Check that now.
	Set authentication = dnoi.ProcessAuthentication(requestUrl, Request.Form)
	If Err.number <> 0 Then
		' Oops, report something that went wrong.
		Response.Write "<p>" + Server.HTMLEncode(Err.Description) + "</p>"
	End If
	On Error Goto 0
	if Not authentication Is Nothing then ' if this WAS an OpenID response coming in...
		If authentication.Successful Then
			Session("ClaimedIdentifier") = authentication.ClaimedIdentifier
			If Not authentication.ClaimsResponse Is Nothing Then
				Session("Email") = authentication.ClaimsResponse.Email
				Session("Nickname") = authentication.ClaimsResponse.Nickname
				Session("FullName") = authentication.ClaimsResponse.FullName
			End If
			Response.Redirect "MembersOnly.asp"
		else
			Response.Write "Authentication failed: " + authentication.ExceptionMessage
		end if
	elseif Request.Form("openid_identifier") <> "" then ' if the user is only now starting the authentication flow...
		dim redirectUrl
		On Error Resume Next
		thisPageUrl = "http://" + Request.ServerVariables("HTTP_HOST") + Request.ServerVariables("URL") ' this is the URL that will receive the response from the OpenID Provider.
		' redirectUrl = dnoi.CreateRequest(Request.Form("openid_identifier"), realm, thisPageUrl)
		redirectUrl = dnoi.CreateRequestWithSimpleRegistration(Request.Form("openid_identifier"), realm, thisPageUrl, "nickname,email", "fullname")
		If Err.number <> 0 Then
			Response.Write "<p>" + Server.HTMLEncode(Err.Description) + "</p>"
		Else
			Response.Redirect redirectUrl
		End If
		On Error Goto 0
	End If 

	%>
	<form action="login.asp" method="post">
	OpenID Login:
	<input class="openid" name="openid_identifier" value="<%=Server.HTMLEncode(Request.Form("openid_identifier"))%>" />
	<input type="submit" value="Login" />
	</form>

	<script>
		document.getElementsByName('openid_identifier')[0].focus();
	</script>

</body>
</html>
