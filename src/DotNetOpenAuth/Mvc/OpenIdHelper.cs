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
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public static class OpenIdHelper {
		public static string OpenIdSelectorScripts(this HtmlHelper html, Page page) {
			return OpenIdSelectorScripts(html, page, null, null);
		}
	
		public static string OpenIdSelectorScripts(this HtmlHelper html, Page page, OpenIdSelector selectorOptions, OpenIdAjaxOptions additionalOptions) {
			Contract.Requires<ArgumentNullException>(html != null);
			Contract.Requires<ArgumentNullException>(page != null);
			Contract.Ensures(Contract.Result<string>() != null);

			if (selectorOptions == null) {
				selectorOptions = new OpenId.RelyingParty.OpenIdSelector();
			}

			if (additionalOptions == null) {
				additionalOptions = new OpenIdAjaxOptions();
			}

			StringWriter result = new StringWriter();

			if (additionalOptions.ShowDiagnosticIFrame || additionalOptions.ShowDiagnosticTrace) {
				result.WriteScriptBlock(string.Format(
					CultureInfo.InvariantCulture,
@"window.openid_visible_iframe = {0}; // causes the hidden iframe to show up
window.openid_trace = {1}; // causes lots of messages",
					additionalOptions.ShowDiagnosticIFrame ? "true" : "false",
					additionalOptions.ShowDiagnosticTrace ? "true" : "false"));
			}
			result.WriteScriptTags(page, new[] {
				OpenIdRelyingPartyControlBase.EmbeddedJavascriptResource,
				OpenIdRelyingPartyAjaxControlBase.EmbeddedAjaxJavascriptResource,
				OpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedScriptResourceName,
			});

			if (selectorOptions.DownloadYahooUILibrary) {
			result.WriteScriptTags(new[] { "https://ajax.googleapis.com/ajax/libs/yui/2.8.0r4/build/yuiloader/yuiloader-min.js" });
			}

			var blockBuilder = new StringWriter();
			if (selectorOptions.DownloadYahooUILibrary) {
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
		if ($('#{1}')[0] && !$('#{1}')[0].value) {{ // popups have no ReturnUrl predefined, but full page LogOn does.
			$('#{1}')[0].setAttribute('value', window.parent.location.href);
		}}
		document.forms[{2}].submit();
	}};",
				additionalOptions.AssertionHiddenFieldId,
				additionalOptions.ReturnUrlHiddenFieldId,
				additionalOptions.FormIndex);

			blockBuilder.WriteLine(@"	$(function () {{
		var box = document.getElementsByName('openid_identifier')[0];
		initAjaxOpenId(box, {0}, {1}, {2}, {3}, {4}, {5},
			null, // js function to invoke on receiving a positive assertion
			{6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17},
			false, // auto postback
			null); // PostBackEventReference (unused in MVC)
	}});",
				MessagingUtilities.GetSafeJavascriptValue(page.ClientScript.GetWebResourceUrl(typeof(OpenIdRelyingPartyControlBase), OpenIdTextBox.EmbeddedLogoResourceName)),
				MessagingUtilities.GetSafeJavascriptValue(page.ClientScript.GetWebResourceUrl(typeof(OpenIdRelyingPartyControlBase), OpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedSpinnerResourceName)),
				MessagingUtilities.GetSafeJavascriptValue(page.ClientScript.GetWebResourceUrl(typeof(OpenIdRelyingPartyControlBase), OpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedLoginSuccessResourceName)),
				MessagingUtilities.GetSafeJavascriptValue(page.ClientScript.GetWebResourceUrl(typeof(OpenIdRelyingPartyControlBase), OpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedLoginFailureResourceName)),
				selectorOptions.Throttle,
				selectorOptions.Timeout.TotalMilliseconds,
				MessagingUtilities.GetSafeJavascriptValue(selectorOptions.TextBox.LogOnText),
				MessagingUtilities.GetSafeJavascriptValue(selectorOptions.TextBox.LogOnToolTip),
				selectorOptions.TextBox.ShowLogOnPostBackButton ? "true" : "false",
				MessagingUtilities.GetSafeJavascriptValue(selectorOptions.TextBox.LogOnPostBackToolTip),
				MessagingUtilities.GetSafeJavascriptValue(selectorOptions.TextBox.RetryText),
				MessagingUtilities.GetSafeJavascriptValue(selectorOptions.TextBox.RetryToolTip),
				MessagingUtilities.GetSafeJavascriptValue(selectorOptions.TextBox.BusyToolTip),
				MessagingUtilities.GetSafeJavascriptValue(selectorOptions.TextBox.IdentifierRequiredMessage),
				MessagingUtilities.GetSafeJavascriptValue(selectorOptions.TextBox.LogOnInProgressMessage),
				MessagingUtilities.GetSafeJavascriptValue(selectorOptions.TextBox.AuthenticationSucceededToolTip),
				MessagingUtilities.GetSafeJavascriptValue(selectorOptions.TextBox.AuthenticatedAsToolTip),
				MessagingUtilities.GetSafeJavascriptValue(selectorOptions.TextBox.AuthenticationFailedToolTip));

			result.WriteScriptBlock(blockBuilder.ToString());
			result.WriteScriptTags(page, OpenId.RelyingParty.OpenIdSelector.EmbeddedScriptResourceName);
			return result.ToString();
		}

		public static string OpenIdSelectorOPButton(this HtmlHelper html, Page page, Identifier providerIdentifier, string imageUrl) {
			Contract.Requires<ArgumentNullException>(html != null);
			Contract.Requires<ArgumentNullException>(page != null);
			Contract.Requires<ArgumentNullException>(providerIdentifier != null);
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(imageUrl));
			Contract.Ensures(Contract.Result<string>() != null);
			
			return OpenIdSelectorButton(html, page, providerIdentifier, "OPButton", imageUrl);
		}

		public static string OpenIdSelectorOpenIdButton(this HtmlHelper html, Page page, string imageUrl) {
			Contract.Requires<ArgumentNullException>(html != null);
			Contract.Requires<ArgumentNullException>(page != null);
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(imageUrl));
			Contract.Ensures(Contract.Result<string>() != null);

			return OpenIdSelectorButton(html, page, "OpenIDButton", "OpenIDButton", imageUrl);
		}

		public static string OpenIdSelector(this HtmlHelper html, Page page, params SelectorButton[] buttons) {
			Contract.Requires<ArgumentNullException>(html != null);
			Contract.Requires<ArgumentNullException>(page != null);
			Contract.Requires<ArgumentNullException>(buttons != null);
			Contract.Ensures(Contract.Result<string>() != null);

			var writer = new StringWriter();
			var h = new HtmlTextWriter(writer);

			h.AddAttribute(HtmlTextWriterAttribute.Class, "OpenIdProviders");
			h.RenderBeginTag(HtmlTextWriterTag.Ul);

			foreach (SelectorButton button in buttons) {
				var op = button as SelectorProviderButton;
				if (op != null) {
					h.Write(OpenIdSelectorOPButton(html, page, op.OPIdentifier, op.Image));
					continue;
				}

				var openid = button as SelectorOpenIdButton;
				if (openid != null) {
					h.Write(OpenIdSelectorOpenIdButton(html, page, openid.Image));
					continue;
				}

				ErrorUtilities.VerifySupported(false, "The {0} button is not yet supported for MVC.", button.GetType().Name);
			}

			h.RenderEndTag(); // ul

			if (buttons.OfType<SelectorOpenIdButton>().Any()) {
				h.Write(OpenIdAjaxTextBox(html));
			}

			return writer.ToString();
		}

		public static string OpenIdAjaxTextBox(this HtmlHelper html) {
			return @"<div style='display: none' id='OpenIDForm'>
		<span class='OpenIdAjaxTextBox' style='display: inline-block; position: relative; font-size: 16px'>
			<input name='openid_identifier' id='openid_identifier' size='40' style='padding-left: 18px; border-style: solid; border-width: 1px; border-color: lightgray' />
		</span>
	</div>";
		}

		private static string OpenIdSelectorButton(this HtmlHelper html, Page page, string id, string cssClass, string imageUrl) {
			Contract.Requires<ArgumentNullException>(html != null);
			Contract.Requires<ArgumentNullException>(page != null);
			Contract.Requires<ArgumentNullException>(id != null);
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(imageUrl));
			Contract.Ensures(Contract.Result<string>() != null);

			var writer = new StringWriter();
			var h = new HtmlTextWriter(writer);

			h.AddAttribute(HtmlTextWriterAttribute.Id, id);
			if (!string.IsNullOrEmpty(cssClass)) {
				h.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
			}
			h.RenderBeginTag(HtmlTextWriterTag.Li);

			h.AddAttribute(HtmlTextWriterAttribute.Href, "#");
			h.RenderBeginTag(HtmlTextWriterTag.A);

			h.RenderBeginTag(HtmlTextWriterTag.Div);
			h.RenderBeginTag(HtmlTextWriterTag.Div);

			h.AddAttribute(HtmlTextWriterAttribute.Src, imageUrl);
			h.RenderBeginTag(HtmlTextWriterTag.Img);
			h.RenderEndTag();

			h.AddAttribute(HtmlTextWriterAttribute.Src, page.ClientScript.GetWebResourceUrl(typeof(OpenIdSelector), OpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedLoginSuccessResourceName));
			h.AddAttribute(HtmlTextWriterAttribute.Class, "loginSuccess");
			h.AddAttribute(HtmlTextWriterAttribute.Title, "Authenticated as {0}");
			h.RenderBeginTag(HtmlTextWriterTag.Img);
			h.RenderEndTag();

			h.RenderEndTag(); // div

			h.AddAttribute(HtmlTextWriterAttribute.Class, "ui-widget-overlay");
			h.RenderBeginTag(HtmlTextWriterTag.Div);
			h.RenderEndTag(); // div

			h.RenderEndTag(); // div
			h.RenderEndTag(); // a
			h.RenderEndTag(); // li

			return writer.ToString();
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
