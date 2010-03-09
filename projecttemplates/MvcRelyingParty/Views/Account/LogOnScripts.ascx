<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="DotNetOpenAuth.Mvc" %>
<script type="text/javascript" src='<%= Url.Content("~/Scripts/MicrosoftAjax.js") %>'></script>
<script type="text/javascript" src='<%= Url.Content("~/Scripts/MicrosoftMvcAjax.js") %>'></script>
<script type="text/javascript" src='<%= Url.Content("~/Scripts/jquery.cookie.js") %>'></script>
<%= Html.OpenIdSelectorScripts(this.Page, new OpenIdSelectorOptions { })%>
