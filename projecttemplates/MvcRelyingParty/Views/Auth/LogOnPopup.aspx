<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="DotNetOpenAuth.Mvc" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Login</title>
	<%= Html.OpenIdSelectorStyles() %>
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
