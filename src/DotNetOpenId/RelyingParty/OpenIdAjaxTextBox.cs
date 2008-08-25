using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
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

		const string authenticationResponseViewStateKey = "AuthenticationResponse";
		const string authDataViewStateKey = "AuthData";
		IAuthenticationResponse authenticationResponse;
		/// <summary>
		/// Gets the completed authentication response.
		/// </summary>
		public IAuthenticationResponse AuthenticationResponse {
			get {
				if (authenticationResponse == null) {
					// We will either validate a new response and return a live AuthenticationResponse
					// or we will try to deserialize a previous IAuthenticationResponse (snapshot)
					// from viewstate and return that.
					IAuthenticationResponse viewstateResponse = ViewState[authenticationResponseViewStateKey] as IAuthenticationResponse;
					string viewstateAuthData = ViewState[authDataViewStateKey] as string;
					string formAuthData = Page.Request.Form["openidAuthData"];

					// First see if there is fresh auth data to be processed into a response.
					if (formAuthData != null && !string.Equals(viewstateAuthData, formAuthData, StringComparison.Ordinal)) {
						ViewState[authDataViewStateKey] = formAuthData;

						Uri authUri = new Uri(formAuthData ?? viewstateAuthData);
						var authDataFields = HttpUtility.ParseQueryString(authUri.Query);
						var rp = new OpenIdRelyingParty(OpenIdRelyingParty.HttpApplicationStore,
							authUri, authDataFields);
						authenticationResponse = rp.Response;

						// Save out the authentication response to viewstate so we can find it on
						// a subsequent postback.
						ViewState[authenticationResponseViewStateKey] = new AuthenticationResponseSnapshot(authenticationResponse);
					} else {
						authenticationResponse = viewstateResponse;
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

		const string timeoutViewStateKey = "Timeout";
		readonly TimeSpan timeoutDefault = TimeSpan.FromSeconds(8);
		/// <summary>
		/// Gets/sets the time duration for the AJAX control to wait for an OP to respond before reporting failure to the user.
		/// </summary>
		[Browsable(true), DefaultValue(typeof(TimeSpan), "00:00:08"), Category("Behavior")]
		[Description("The time duration for the AJAX control to wait for an OP to respond before reporting failure to the user.")]
		public TimeSpan Timeout {
			get { return (TimeSpan)(ViewState[timeoutViewStateKey] ?? timeoutDefault); }
			set {
				if (value.TotalMilliseconds <= 0) throw new ArgumentOutOfRangeException("value");
				ViewState[timeoutViewStateKey] = value;
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

		bool focusCalled;
		/// <summary>
		/// Places focus on the text box when the page is rendered on the browser.
		/// </summary>
		public override void Focus() {
			focusCalled = true;
			// we don't emit the code to focus the control immediately, in case the control
			// is never rendered to the page because its Visible property is false or that
			// of any of its parent containers.
		}

		/// <summary>
		/// Fired when the user has typed in their identifier, discovery was successful
		/// and a login attempt is about to begin.
		/// </summary>
		[Description("Fired when the user has typed in their identifier, discovery was successful and a login attempt is about to begin.")]
		public event EventHandler<OpenIdEventArgs> LoggingIn;
		protected virtual void OnLoggingIn(IAuthenticationRequest request) {
			var loggingIn = LoggingIn;
			if (loggingIn != null) {
				loggingIn(this, new OpenIdEventArgs(request));
			}
		}

		/// <summary>
		/// Fired when authentication has completed successfully.
		/// </summary>
		[Description("Fired when authentication has completed successfully.")]
		public event EventHandler<OpenIdEventArgs> LoggedIn;
		protected virtual void OnLoggedIn(IAuthenticationResponse response) {
			var loggedIn = LoggedIn;
			if (loggedIn != null) {
				loggedIn(this, new OpenIdEventArgs(response));
			}
		}

		/// <summary>
		/// Prepares the control for loading.
		/// </summary>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (Page.IsPostBack) {
				// If the control was temporarily hidden, it won't be in the Form data,
				// and we'll just implicitly keep the last Text setting.
				if (Page.Request.Form[Name] != null) {
					Text = Page.Request.Form[Name];
				}

				// If there is a response, and it is fresh (live object, not a snapshot object)...
				if (AuthenticationResponse != null && AuthenticationResponse is AuthenticationResponse) {
					switch (AuthenticationResponse.Status) {
						case AuthenticationStatus.Authenticated:
							OnLoggedIn(AuthenticationResponse);
							break;
						default:
							break;
					}
				}
			} else {
				string userSuppliedIdentifier = Page.Request.QueryString["dotnetopenid.userSuppliedIdentifier"];
				if (!string.IsNullOrEmpty(userSuppliedIdentifier)) {
					Logger.Info("AJAX (iframe) request detected.");
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
			// Call into the .js file with initialization information.
			StringBuilder startupScript = new StringBuilder();
			startupScript.AppendLine("<script language='javascript'>");
			startupScript.AppendFormat("var box = document.getElementsByName('{0}')[0];{1}", Name, Environment.NewLine);
			if (focusCalled) {
				startupScript.AppendLine("box.focus();");
			}
			startupScript.AppendFormat("initAjaxOpenId(box, '{0}', '{1}', {2});{3}",
				Page.ClientScript.GetWebResourceUrl(GetType(), EmbeddedDotNetOpenIdLogoResourceName),
				Page.ClientScript.GetWebResourceUrl(GetType(), EmbeddedSpinnerResourceName),
				Timeout.TotalMilliseconds,
				Environment.NewLine);

			if (AuthenticationResponse != null && AuthenticationResponse.Status == AuthenticationStatus.Authenticated) {
				startupScript.AppendFormat("box.openidAuthResult('{0}');{1}", ViewState[authDataViewStateKey].ToString().Replace("'", "\\'"), Environment.NewLine);
			}
			startupScript.AppendLine("</script>");

			Page.ClientScript.RegisterStartupScript(GetType(), "ajaxstartup", startupScript.ToString());
			Page.ClientScript.RegisterOnSubmitStatement(GetType(), "loginvalidation", string.Format(CultureInfo.InvariantCulture, @"
var openidbox = document.getElementsByName('{0}')[0];
if (!openidbox.onSubmit()) {{ return false; }}
", Name));
		}

		private void performDiscovery(string userSuppliedIdentifier) {
			Logger.InfoFormat("Discovery on {0} requested.", userSuppliedIdentifier);
			OpenIdRelyingParty rp = new OpenIdRelyingParty();

			try {
				IAuthenticationRequest req = rp.CreateRequest(userSuppliedIdentifier);
				req.AddCallbackArguments("dotnetopenid.phase", "2");
				if (Page.Request.QueryString["dotnetopenid.immediate"] == "true") {
					req.Mode = AuthenticationRequestMode.Immediate;
				}
				OnLoggingIn(req);
				req.RedirectToProvider();
			} catch (OpenIdException ex) {
				callbackUserAgentMethod("openidDiscoveryFailure('" + ex.Message.Replace("'", "\\'") + "')");
			}
		}

		private void reportDiscoveryResult() {
			Logger.InfoFormat("AJAX (iframe) callback from OP: {0}", Page.Request.Url);
			callbackUserAgentMethod("openidAuthResult(document.URL)");
		}

		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			prepareClientJavascript();
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

		/// <summary>
		/// Invokes a method on a parent frame/window's OpenIdAjaxTextBox,
		/// and closes the calling popup window if applicable.
		/// </summary>
		/// <param name="methodCall">The method to call on the OpenIdAjaxTextBox, including
		/// parameters.  (i.e. "callback('arg1', 2)").  No escaping is done by this method.</param>
		private void callbackUserAgentMethod(string methodCall) {
			Logger.InfoFormat("Sending Javascript callback: {0}", methodCall);
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
