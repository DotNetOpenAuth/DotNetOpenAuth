<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>


<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Login</title>
	<link rel="Stylesheet" type="text/css" href="<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector.css")%>" />
	<link rel="Stylesheet" type="text/css" href="<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.OpenIdAjaxTextBox.css")%>" />
	<link rel="stylesheet" type="text/css" href='<%= Url.Content("~/Content/loginpopup.css") %>' />
</head>
<body>
<% Html.RenderPartial("LogOnContent"); %>
	<% if (Request.Url.IsLoopback) { %>
		<script type="text/javascript" src='<%= Url.Content("~/Scripts/jquery-1.3.2.min.js") %>'></script>
		<script type="text/javascript" src='<%= Url.Content("~/Scripts/jquery-ui-personalized-1.6rc6.min.js") %>'></script>
	<% } else { %>
		<script type="text/javascript" language="javascript" src="http://www.google.com/jsapi"></script>
		<script type="text/javascript" language="javascript">
			google.load("jquery", "1.3.2");
			google.load("jqueryui", "1.7.2");
		</script>
	<% } %>
<% Html.RenderPartial("LogOnScripts"); %>
</body>
</html>
