<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>DotNetOpenAuth Classic ASP sample: Login</title>
	<link href="styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
	<div>
		<a href="http://DotNetOpenId.googlecode.com">
			<img runat="server" src="images/DotNetOpenId_tiny.gif" title="Jump to the project web site."
				alt="DotNetOpenAuth" border='0' /></a>
	</div>
	<h2>Login Page</h2>
	<%
	dim realm, thisPageUrl, requestUrl, dnoi, authentication
	realm = "http://" + Request.ServerVariables("HTTP_HOST") + "/classicaspdnoi/"
	thisPageUrl = "http://" + Request.ServerVariables("HTTP_HOST") + Request.ServerVariables("URL")
	requestUrl = "http://" + Request.ServerVariables("HTTP_HOST") + Request.ServerVariables("HTTP_URL")
	Set dnoi = server.CreateObject("DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingParty")
	Set authentication = dnoi.ProcessAuthentication(requestUrl, Request.Form)
	if Not authentication Is Nothing then
		If authentication.Successful Then
			Session("ClaimedIdentifier") = authentication.ClaimedIdentifier
			Session("Email") = authentication.ClaimsResponse.Email
			Session("Nickname") = authentication.ClaimsResponse.Nickname
			Session("FullName") = authentication.ClaimsResponse.FullName
			Response.Redirect "MembersOnly.asp"
		else
			Response.Write "Authentication failed: " + authentication.ExceptionMessage
		end if
	elseif Request.Form("openid_identifier") <> "" then
		dim redirectUrl
		' redirectUrl = dnoi.CreateRequest(Request.Form("openid_identifier"), realm, thisPageUrl)
		redirectUrl = dnoi.CreateRequestWithSimpleRegistration(Request.Form("openid_identifier"), realm, thisPageUrl, "nickname,email", "fullname")
		Response.Redirect redirectUrl
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
