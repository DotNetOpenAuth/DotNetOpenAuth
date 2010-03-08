<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<script type="text/javascript" language="javascript"><!--
	//<![CDATA[
	//window.openid_visible_iframe = true; // causes the hidden iframe to show up
	//window.openid_trace = true; // causes lots of messages
//]]>--></script>
<script type="text/javascript" src='<%= Url.Content("~/Scripts/MicrosoftAjax.js") %>'></script>
<script type="text/javascript" src='<%= Url.Content("~/Scripts/MicrosoftMvcAjax.js") %>'></script>
<script type="text/javascript" src='<%= Url.Content("~/Scripts/jquery.cookie.js") %>'></script>
<script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/yui/2.8.0r4/build/yuiloader/yuiloader-min.js"></script>
<script type="text/javascript" src="<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingPartyControlBase.js")%>"></script>
<script type="text/javascript" src="<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingPartyAjaxControlBase.js")%>"></script>
<script type="text/javascript" src="<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.OpenIdAjaxTextBox.js")%>"></script>
<script type="text/javascript" language="javascript"><!--
	//<![CDATA[
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
	window.postLoginAssertion = function (positiveAssertion) {
		$('#openid_openidAuthData')[0].setAttribute('value', positiveAssertion);
		if (!$('#ReturnUrl')[0].value) { // popups have no ReturnUrl predefined, but full page LogOn does.
			$('#ReturnUrl')[0].setAttribute('value', window.parent.location.href);
		}
		document.forms[0].submit();
	};
	$(function () {
		var box = document.getElementsByName('openid_identifier')[0];
		initAjaxOpenId(
			box,
			'<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.openid_login.gif")%>',
			'<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.spinner.gif")%>',
			'<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.login_success.png")%>',
			'<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.login_failure.png")%>',
			3, // throttle
			8000, // timeout
			null, // js function to invoke on receiving a positive assertion
			"LOG IN",
			"Click here to log in using a pop-up window.",
			true, // ShowLogOnPostBackButton
			"Click here to log in immediately.",
			"RETRY",
			"Retry a failed identifier discovery.",
			"Discovering/authenticating",
			"Please correct errors in OpenID identifier and allow login to complete before submitting.",
			"Please wait for login to complete.",
			"Authenticated by {0}.",
			"Authenticated as {0}.",
			"Authentication failed.",
			false, // auto postback
			null); // PostBackEventReference (unused in MVC)
	});
//]]>--></script>
<script type="text/javascript" src="<%=Page.ClientScript.GetWebResourceUrl(typeof(DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector), "DotNetOpenAuth.OpenId.RelyingParty.OpenIdSelector.js")%>"></script>
