<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="OpenIdWebRingSsoRelyingParty._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Sample SSO relying party</title>
</head>
<body>
	<form id="form1" runat="server">
	<div>
		We&#39;ve recognized you (via the SSO OP) as:
		<asp:LoginName ID="LoginName1" runat="server" />
		<p>Try visiting the <a href="Admin/Default.aspx">Admin area</a></p>
	</div>
	<p>This sample is of an OpenID Relying Party that acts within a controlled set of 
		web sites (perhaps all belonging to the same organization).&nbsp; This 
		particular RP is configured to require authentication for all web pages, and to 
		always use just one (trusted) OP (the OpenIdWebRingSsoProvider) without ever 
		prompting the user.</p>
	<p>Although the sample OP uses Windows Authentication, and so this RP could easily 
		do the same, the idea is that the OP and RP may exist on different network 
		topologies, or the OP may be the only site with access to the user credential 
		database, or any number of other scenarios where the RP doesn&#39;t have the freedom 
		to authenticate the user the way the OP has, yet this set of web sites want to 
		have the users only authenticate themselves to one site with one set of 
		credentials.</p>
	</form>
</body>
</html>
