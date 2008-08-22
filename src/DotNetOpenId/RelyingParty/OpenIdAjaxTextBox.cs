using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedScriptResourceName, "text/javascript")]
[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedDotNetOpenIdLogoResourceName, "image/gif")]
[assembly: WebResource(DotNetOpenId.RelyingParty.OpenIdAjaxTextBox.EmbeddedSpinnerResourceName, "image/gif")]

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// An ASP.NET control that provides a minimal text box that is OpenID-aware and uses AJAX for
	/// a premium login experience.
	/// </summary>
	[DefaultProperty("Text"), ValidationProperty("Text")]
	[ToolboxData("<{0}:OpenIdAjaxTextBox runat=\"server\" />")]
	public class OpenIdAjaxTextBox : WebControl {
		internal const string EmbeddedScriptResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.OpenIdAjaxTextBox.js";
		internal const string EmbeddedDotNetOpenIdLogoResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.dotnetopenid_16x16.gif";
		internal const string EmbeddedSpinnerResourceName = DotNetOpenId.Util.DefaultNamespace + ".RelyingParty.spinner.gif";

		#region Properties

		IAuthenticationResponse authenticationResponse;
		/// <summary>
		/// Gets the completed authentication response in a postback.
		/// </summary>
		/// <remarks>
		/// Calling this property's getter causes the authentication process to complete,
		/// which prevents the authentication from completing (again) on any subsequent postback.
		/// Only call this property from the host page when ready to handle completed authentication
		/// and store the result somewhere so subsequent postbacks or pages on the hosting site
		/// can access the authentication result.
		/// </remarks>
		public IAuthenticationResponse AuthenticationResponse {
			get {
				if (authenticationResponse == null) {
					string authData = Page.Request.Form["openidAuthData"];
					if (!string.IsNullOrEmpty(authData)) {
						var authDataFields = HttpUtility.ParseQueryString(authData);
						// Lie about the request URL so it matches the return_to URL made
						// back when this authentication occurred.
						Uri returnTo = getReturnTo(Util.GetRequestUrlFromContext(), authDataFields);
						var rp = new OpenIdRelyingParty(OpenIdRelyingParty.HttpApplicationStore,
							returnTo, authDataFields);
						authenticationResponse = rp.Response;
					}
				}
				return authenticationResponse;
			}
		}

		const string textViewStateKey = "Text";
		/// <summary>
		/// Gets/sets the value in the text field, completely unprocessed or normalized.
		/// </summary>
		[Bindable(true), DefaultValue(""), Category("Appearance")]
		[Description("The value in the text field, completely unprocessed or normalized.")]
		public string Text {
			get { return (string)(ViewState[textViewStateKey] ?? string.Empty); }
			set { ViewState[textViewStateKey] = value ?? string.Empty; }
		}

		const string columnsViewStateKey = "Columns";
		const int columnsDefault = 40;
		/// <summary>
		/// The width of the text box in characters.
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(columnsDefault)]
		[Description("The width of the text box in characters.")]
		public int Columns {
			get { return (int)(ViewState[columnsViewStateKey] ?? columnsDefault); }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("value");
				ViewState[columnsViewStateKey] = value;
			}
		}

		const string tabIndexViewStateKey = "TabIndex";
		/// <summary>
		/// Default value for <see cref="TabIndex"/> property.
		/// </summary>
		const short tabIndexDefault = 0;
		/// <summary>
		/// The tab index of the text box control.  Use 0 to omit an explicit tabindex.
		/// </summary>
		[Bindable(true), Category("Behavior"), DefaultValue(tabIndexDefault)]
		[Description("The tab index of the text box control.  Use 0 to omit an explicit tabindex.")]
		public override short TabIndex {
			get { return (short)(ViewState[tabIndexViewStateKey] ?? tabIndexDefault); }
			set { ViewState[tabIndexViewStateKey] = value; }
		}

		const string nameViewStateKey = "Name";
		const string nameDefault = "openid_identifier";
		/// <summary>
		/// Gets/sets the HTML name to assign to the text field.
		/// </summary>
		[Bindable(true), DefaultValue(nameDefault), Category("Misc")]
		[Description("The HTML name to assign to the text field.")]
		public string Name {
			get { return (string)(ViewState[nameViewStateKey] ?? nameDefault); }
			set {
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("value");
				ViewState[nameViewStateKey] = value ?? string.Empty;
			}
		}

		const string logonTextViewStateKey = "LoginText";
		const string logonTextDefault = "LOG IN";
		/// <summary>
		/// Gets/sets the text that appears on the LOG IN button in cases where immediate (invisible) authentication fails.
		/// </summary>
		[Bindable(true), DefaultValue(logonTextDefault), Localizable(true), Category("Appearance")]
		[Description("The text that appears on the LOG IN button in cases where immediate (invisible) authentication fails.")]
		public string LogOnText {
			get { return (string)(ViewState[logonTextViewStateKey] ?? logonTextDefault); }
			set {
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("value");
				ViewState[logonTextViewStateKey] = value ?? string.Empty;
			}
		}

		#endregion

		#region Properties to hide
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color ForeColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BackColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BorderColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override Unit BorderWidth {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override BorderStyle BorderStyle {
			get { return BorderStyle.None; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override FontInfo Font {
			get { return null; }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override Unit Height {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override Unit Width {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override string ToolTip {
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override string SkinID {
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Unused property.
		/// </summary>
		[Browsable(false), Bindable(false)]
		public override bool EnableTheming {
			get { return false; }
			set { throw new NotSupportedException(); }
		}
		#endregion

		/// <summary>
		/// Places focus on the text box when the page is rendered on the browser.
		/// </summary>
		public override void Focus() {
			Page.ClientScript.RegisterStartupScript(GetType(), "focus", string.Format(CultureInfo.InvariantCulture, @"
<script language='javascript'>
document.getElementsByName({0})[0].focus();
</script>", Name));
		}

		/// <summary>
		/// Prepares the control for loading.
		/// </summary>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			prepareClientJavascript();
			if (!Page.IsPostBack) {
				string userSuppliedIdentifier = Page.Request.QueryString["dotnetopenid.userSuppliedIdentifier"];
				if (!string.IsNullOrEmpty(userSuppliedIdentifier)) {
					if (Page.Request.QueryString["dotnetopenid.phase"] == "2") {
						reportDiscoveryResult();
					} else {
						performDiscovery(userSuppliedIdentifier);
					}
				}
			}
		}

		private void prepareClientJavascript() {
			// Import the .js file where most of the code is.
			Page.ClientScript.RegisterClientScriptResource(typeof(OpenIdAjaxTextBox), EmbeddedScriptResourceName);
			// Cal into the .js file with initialization information.
			Page.ClientScript.RegisterStartupScript(GetType(), "ajaxstartup", string.Format(CultureInfo.InvariantCulture, @"
<script language='javascript'>
var openidbox = document.getElementsByName('{0}')[0];
if (openidbox) {{ initAjaxOpenId(openidbox, '{1}', '{2}'); }}
</script>", Name,
				Page.ClientScript.GetWebResourceUrl(GetType(), EmbeddedDotNetOpenIdLogoResourceName),
				Page.ClientScript.GetWebResourceUrl(GetType(), EmbeddedSpinnerResourceName)));
		}

		private void performDiscovery(string userSuppliedIdentifier) {
			OpenIdRelyingParty rp = new OpenIdRelyingParty();

			try {
				IAuthenticationRequest req = rp.CreateRequest(userSuppliedIdentifier);
				req.AddCallbackArguments("dotnetopenid.phase", "2");
				if (Page.Request.QueryString["dotnetopenid.immediate"] == "true") {
					req.Mode = AuthenticationRequestMode.Immediate;
				}
				req.RedirectToProvider();
			} catch (OpenIdException ex) {
				callbackUserAgentMethod("openidDiscoveryFailure('" + ex.Message.Replace("'", "\\'") + "')");
			}
		}

		private void reportDiscoveryResult() {
			callbackUserAgentMethod("openidAuthResult(document.URL)");
		}

		/// <summary>
		/// Renders the control.
		/// </summary>
		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			string logoUrl = Page.ClientScript.GetWebResourceUrl(
				typeof(OpenIdTextBox), OpenIdTextBox.EmbeddedLogoResourceName);

			// We surround the textbox with a span so that the .js file can inject a
			// login button within the text box with easy placement.
			writer.WriteBeginTag("span");
			writer.Write(" style='");
			writer.WriteStyleAttribute("position", "relative");
			writer.Write("'>");

			writer.WriteBeginTag("input");
			writer.WriteAttribute("name", Name);
			writer.WriteAttribute("value", Text);
			writer.WriteAttribute("size", Columns.ToString(CultureInfo.InvariantCulture));
			if (TabIndex > 0) {
				writer.WriteAttribute("tabindex", TabIndex.ToString(CultureInfo.InvariantCulture));
			}
			if (!Enabled) {
				writer.WriteAttribute("disabled", "true");
			}
			if (!string.IsNullOrEmpty(CssClass)) {
				writer.WriteAttribute("class", CssClass);
			}
			writer.Write(" style='");
			writer.WriteStyleAttribute("background", string.Format(CultureInfo.InvariantCulture,
					"url({0}) no-repeat", HttpUtility.HtmlEncode(logoUrl)));
			writer.WriteStyleAttribute("background-position", "0 50%");
			writer.WriteStyleAttribute("padding-left", "18px");
			writer.WriteStyleAttribute("border-style", "solid");
			writer.WriteStyleAttribute("border-width", "1px");
			writer.WriteStyleAttribute("border-color", "lightgray");
			writer.Write("'");
			writer.Write(" />");

			writer.WriteEndTag("span");
		}

		private static Uri getReturnTo(Uri uri, System.Collections.Specialized.NameValueCollection authDataFields) {
			UriBuilder builder = new UriBuilder(uri);
			NameValueCollection returnToNVC = HttpUtility.ParseQueryString(builder.Query);
			foreach (string key in authDataFields) {
				if (returnToNVC[key] == null) {
					returnToNVC[key] = authDataFields[key];
				} else {
					if (returnToNVC[key] != authDataFields[key]) {
						throw new ArgumentException(Strings.ReturnToParamDoesNotMatchRequestUrl, key);
					}
				}
			}
			builder.Query = UriUtil.CreateQueryString(returnToNVC);
			return builder.Uri;
		}

		/// <summary>
		/// Invokes a method on a parent frame/window's OpenIdAjaxTextBox,
		/// and closes the calling popup window if applicable.
		/// </summary>
		/// <param name="methodCall">The method to call on the OpenIdAjaxTextBox, including
		/// parameters.  (i.e. "callback('arg1', 2)").  No escaping is done by this method.</param>
		private void callbackUserAgentMethod(string methodCall) {
			Page.Response.Write(string.Format(CultureInfo.InvariantCulture,
				@"<html><body><script language='javascript'>
					var objSrc = window.frameElement ? window.frameElement.openidBox : window.opener.waiting_openidBox;
					objSrc.{0};
					if (!window.frameElement) {{ self.close(); }}
				</script></body></html>", methodCall));
			Page.Response.End();
		}
	}
}
