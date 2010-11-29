<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="DotNetOpenAuth.Mvc" %>
<script type="text/javascript" src='<%= Url.Content("~/Scripts/MicrosoftAjax.js") %>'></script>
<script type="text/javascript" src='<%= Url.Content("~/Scripts/MicrosoftMvcAjax.js") %>'></script>
<script type="text/javascript" src='<%= Url.Content("~/Scripts/jquery.cookie.js") %>'></script>
<%
	var options = new OpenIdAjaxOptions {
		PreloadedDiscoveryResults = (string)this.ViewData["PreloadedDiscoveryResults"],
	};
%>
<%= Html.OpenIdSelectorScripts(null, options)%>
