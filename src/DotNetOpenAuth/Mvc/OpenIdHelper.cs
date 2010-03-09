//-----------------------------------------------------------------------
// <copyright file="OpenIdHelper.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Mvc {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using System.Web.UI;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public static class OpenIdHelper {
		public static string OpenIdSelectorScripts(this HtmlHelper html, Page page, OpenIdSelectorOptions options) {
			Contract.Requires<ArgumentNullException>(html != null);
			Contract.Requires<ArgumentNullException>(page != null);
			Contract.Requires<ArgumentNullException>(options != null);

			StringWriter result = new StringWriter();

			if (options.ShowDiagnosticIFrame || options.ShowDiagnosticTrace) {
				result.WriteScriptBlock(string.Format(
					CultureInfo.InvariantCulture,
@"window.openid_visible_iframe = {0}; // causes the hidden iframe to show up
window.openid_trace = {1}; // causes lots of messages",
					options.ShowDiagnosticIFrame ? "true" : "false",
					options.ShowDiagnosticTrace ? "true" : "false"));
			}
			result.WriteScriptTags(page, new[] {
				OpenIdRelyingPartyControlBase.EmbeddedJavascriptResource,
				OpenIdRelyingPartyAjaxControlBase.EmbeddedAjaxJavascriptResource,
				OpenIdAjaxTextBox.EmbeddedScriptResourceName,
			});

			if (options.DownloadYahooUILibrary) {
			result.WriteScriptTags(new[] { "https://ajax.googleapis.com/ajax/libs/yui/2.8.0r4/build/yuiloader/yuiloader-min.js" });
			}

			var blockBuilder = new StringWriter();
			if (options.DownloadYahooUILibrary) {
				blockBuilder.WriteLine(@"	try {
		if (YAHOO) {
			var loader = new YAHOO.util.YUILoader({
				require: ['button', 'menu'],
				loadOptional: false,
				combine: true
			});

			loader.insert();
		}
	} catch (e) { }");
			}

			blockBuilder.WriteLine("window.aspnetapppath = '{0}';", VirtualPathUtility.AppendTrailingSlash(HttpContext.Current.Request.ApplicationPath));

			// Positive assertions can last no longer than this library is willing to consider them valid,
			// and when they come with OP private associations they last no longer than the OP is willing
			// to consider them valid.  We assume the OP will hold them valid for at least five minutes.
			double assertionLifetimeInMilliseconds = Math.Min(TimeSpan.FromMinutes(5).TotalMilliseconds, Math.Min(DotNetOpenAuthSection.Configuration.OpenId.MaxAuthenticationTime.TotalMilliseconds, DotNetOpenAuthSection.Configuration.Messaging.MaximumMessageLifetime.TotalMilliseconds));
			blockBuilder.WriteLine(
				"{0} = {1};",
				OpenIdRelyingPartyAjaxControlBase.MaxPositiveAssertionLifetimeJsName,
				assertionLifetimeInMilliseconds.ToString(CultureInfo.InvariantCulture));

			string discoverUrl = VirtualPathUtility.AppendTrailingSlash(HttpContext.Current.Request.ApplicationPath) + html.RouteCollection["OpenIdDiscover"].GetVirtualPath(html.ViewContext.RequestContext, new RouteValueDictionary(new { identifier = "xxx" })).VirtualPath;
			blockBuilder.WriteLine(@"	{0} = function (argument, resultFunction, errorCallback) {{
		jQuery.ajax({{
			async: true,
			dataType: 'text',
			error: function (request, status, error) {{ errorCallback(status, argument); }},
			success: function (result) {{ resultFunction(result, argument); }},
			url: '{1}'.replace('xxx', encodeURIComponent(argument))
		}});
	}};",
				OpenIdRelyingPartyAjaxControlBase.CallbackJSFunctionAsync,
				discoverUrl);

			blockBuilder.WriteLine(@"	window.postLoginAssertion = function (positiveAssertion) {{
		$('#{0}')[0].setAttribute('value', positiveAssertion);
		if (!$('#ReturnUrl')[0].value) {{ // popups have no ReturnUrl predefined, but full page LogOn does.
			$('#ReturnUrl')[0].setAttribute('value', window.parent.location.href);
		}}
		document.forms[0].submit();
	}};",
				options.AssertionHiddenFieldId);

			blockBuilder.WriteLine(@"	$(function () {{
		var box = document.getElementsByName('openid_identifier')[0];
		initAjaxOpenId(
			box,
			{0},
			{1},
			{2},
			{3},
			3, // throttle
			8000, // timeout
			null, // js function to invoke on receiving a positive assertion
			'LOG IN',
			'Click here to log in using a pop-up window.',
			true, // ShowLogOnPostBackButton
			'Click here to log in immediately.',
			'RETRY',
			'Retry a failed identifier discovery.',
			'Discovering/authenticating',
			'Please correct errors in OpenID identifier and allow login to complete before submitting.',
			'Please wait for login to complete.',
			'Authenticated by {{0}}.',
			'Authenticated as {{0}}.',
			'Authentication failed.',
			false, // auto postback
			null); // PostBackEventReference (unused in MVC)
	}});",
				MessagingUtilities.GetSafeJavascriptValue(page.ClientScript.GetWebResourceUrl(typeof(OpenIdRelyingPartyControlBase), OpenIdTextBox.EmbeddedLogoResourceName)),
				MessagingUtilities.GetSafeJavascriptValue(page.ClientScript.GetWebResourceUrl(typeof(OpenIdRelyingPartyControlBase), OpenIdAjaxTextBox.EmbeddedSpinnerResourceName)),
				MessagingUtilities.GetSafeJavascriptValue(page.ClientScript.GetWebResourceUrl(typeof(OpenIdRelyingPartyControlBase), OpenIdAjaxTextBox.EmbeddedLoginSuccessResourceName)),
				MessagingUtilities.GetSafeJavascriptValue(page.ClientScript.GetWebResourceUrl(typeof(OpenIdRelyingPartyControlBase), OpenIdAjaxTextBox.EmbeddedLoginFailureResourceName)));

			result.WriteScriptBlock(blockBuilder.ToString());
			result.WriteScriptTags(page, OpenIdSelector.EmbeddedScriptResourceName);
			return result.ToString();
		}

		private static void WriteScriptTags(this TextWriter writer, IEnumerable<string> scriptUrls) {
			Contract.Requires<ArgumentNullException>(writer != null);
			Contract.Requires<ArgumentNullException>(scriptUrls != null);

			foreach (string script in scriptUrls) {
				writer.WriteLine("<script type='text/javascript' src='{0}'></script>", script);
			}
		}

		private static void WriteScriptTags(this TextWriter writer, Page page, string resourceName) {
			WriteScriptTags(writer, page, new[] { resourceName });
		}

		private static void WriteScriptTags(this TextWriter writer, Page page, IEnumerable<string> resourceNames) {
			Contract.Requires<ArgumentNullException>(writer != null);
			Contract.Requires<ArgumentNullException>(page != null);
			Contract.Requires<ArgumentNullException>(resourceNames != null);

			writer.WriteScriptTags(resourceNames.Select(r => page.ClientScript.GetWebResourceUrl(typeof(OpenIdRelyingPartyControlBase), r)));
		}

		private static void WriteScriptBlock(this TextWriter writer, string script) {
			writer.WriteLine("<script type='text/javascript' language='javascript'><!--");
			writer.WriteLine("//<![CDATA[");
			writer.WriteLine(script);
			writer.WriteLine("//]]>--></script>");
		}
	}
}
