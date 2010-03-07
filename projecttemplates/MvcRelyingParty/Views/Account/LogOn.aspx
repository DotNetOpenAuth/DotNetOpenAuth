<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="loginTitle" ContentPlaceHolderID="TitleContent" runat="server">
	Log On
</asp:Content>
<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		Log On
	</h2>
	<%= Html.ValidationSummary("Login was unsuccessful. Please correct the errors and try again.") %>

	<% using (Html.BeginForm("LogOn", "Account")) { %>
	<%= Html.AntiForgeryToken() %>
	<%= Html.Hidden("ReturnUrl", Request.QueryString["ReturnUrl"]) %>
	<%= Html.Hidden("openid_openidAuthData") %>
	<div>
		<fieldset>
			<legend>Account Information</legend>
			<ul class="OpenIdProviders">
				<li id="https://www.google.com/accounts/o8/id" class="OPButton"><a href="#"><div><div>
				<img src="../../Content/images/google.gif" />
				<img src="<%= Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.login_success.png") %>" class="loginSuccess" title="Authenticated as {0}" />
				</div><div class="ui-widget-overlay"></div></div></a>
				</li>
				<li id="https://me.yahoo.com/" class="OPButton"><a href="#"><div><div>
				<img src="../../Content/images/yahoo.gif" />
				<img src="<%= Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.login_success.png") %>" class="loginSuccess" title="Authenticated as {0}" />
				</div><div class="ui-widget-overlay"></div></div></a>
				</li>
			</ul>
			<p>
				<%= Html.CheckBox("rememberMe") %> <label class="inline" for="rememberMe">Remember me?</label>
			</p>
			<p>
				<input type="submit" value="Log On" />
			</p>
		</fieldset>
	</div>
	<% } %>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ScriptsArea" runat="server">
	<script type="text/javascript" language="javascript"><!--
		//<![CDATA[
		window.openid_visible_iframe = true; // causes the hidden iframe to show up
		window.openid_trace = true; // causes lots of messages
	//]]>--></script>
	<script type="text/javascript" src="../../Scripts/MicrosoftAjax.js"></script>
	<script type="text/javascript" src="../../Scripts/MicrosoftMvcAjax.js"></script>
	<script type="text/javascript" src="../../Scripts/jquery-1.3.2.min.js"></script>
	<script type="text/javascript" src="../../Scripts/jquery.cookie.js"></script>
	<script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/yui/2.8.0r4/build/yuiloader/yuiloader-min.js"></script>
	<script type="text/javascript" src="<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingPartyControlBase.js")%>"></script>
	<script type="text/javascript" src="<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingPartyAjaxControlBase.js")%>"></script>
	<script type="text/javascript" src="<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector.js")%>"></script>
	<script type="text/javascript" language="javascript"><!--
		//<![CDATA[
		//$addHandler(window, 'load', function () { document.getElementsByName("openid_identifier")[0].focus(); });
		try {
			if (YAHOO) {
				var loader = new YAHOO.util.YUILoader({
					require: ['button', 'menu'],
					loadOptional: false,
					combine: true
				});

				loader.insert();
			}
		} catch (e) { }
		window.aspnetapppath = '/';
		window.dnoa_internal.maxPositiveAssertionLifetime = 5 * 60 * 1000;
		window.dnoa_internal.callbackAsync = function (argument, resultFunction, errorCallback) {
			var req = new Sys.Net.WebRequest();
			jQuery.ajax({
				async: true,
				dataType: "text",
				error: function (request, status, error) { errorCallback(status, argument); },
				success: function (result) { resultFunction(result, argument); },
				url: '<%= Url.Action("Discover") %>?identifier=' + encodeURIComponent(argument)
			});
		};
		window.dnoa_internal.callback = function (argument, resultFunction, errorCallback) {
			alert('we thought this was unused');
		};
	//]]>--></script>
</asp:Content>
<asp:Content ContentPlaceHolderID="Head" runat="server">
	<link rel="Stylesheet" type="text/css" href="<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector.css")%>" />
</asp:Content>
