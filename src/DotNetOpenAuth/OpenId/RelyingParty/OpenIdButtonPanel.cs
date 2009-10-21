//-----------------------------------------------------------------------
// <copyright file="OpenIdButtonPanel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdButtonPanel.EmbeddedScriptResourceName, "text/javascript")]
[assembly: System.Web.UI.WebResource(DotNetOpenAuth.OpenId.RelyingParty.OpenIdButtonPanel.EmbeddedStylesheetResourceName, "text/css")]

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.ComponentModel;
	using DotNetOpenAuth.Messaging;

	[ToolboxData("<{0}:OpenIdButtonPanel runat=\"server\"></{0}:OpenIdButtonPanel>")]
	[ParseChildren(true), PersistChildren(false)]
	public class OpenIdButtonPanel : OpenIdRelyingPartyAjaxControlBase {
		/// <summary>
		/// The name of the manifest stream containing the OpenIdButtonPanel.js file.
		/// </summary>
		internal const string EmbeddedScriptResourceName = Util.DefaultNamespace + ".OpenId.RelyingParty.OpenIdButtonPanel.js";

		/// <summary>
		/// The name of the manifest stream containing the OpenIdButtonPanel.css file.
		/// </summary>
		internal const string EmbeddedStylesheetResourceName = Util.DefaultNamespace + ".OpenId.RelyingParty.OpenIdButtonPanel.css";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="Providers"/> property.
		/// </summary>
		private const string ProvidersViewStateKey = "Providers";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="AuthenticatedAsToolTip"/> property.
		/// </summary>
		private const string AuthenticatedAsToolTipViewStateKey = "AuthenticatedAsToolTip";

		/// <summary>
		/// The default value for the <see cref="AuthenticatedAsToolTip"/> property.
		/// </summary>
		private const string AuthenticatedAsToolTipDefault = "We recognize you!";

		/// <summary>
		/// The OpenIdAjaxTextBox that remains hidden until the user clicks the OpenID button.
		/// </summary>
		private OpenIdAjaxTextBox textBox;

		/// <summary>
		/// The hidden field that will transmit the positive assertion to the RP.
		/// </summary>
		private HiddenField positiveAssertionField;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdButtonPanel"/> class.
		/// </summary>
		public OpenIdButtonPanel() {
		}

		/// <summary>
		/// Gets or sets the tool tip text that appears on the green checkmark when authentication succeeds.
		/// </summary>
		[Bindable(true), DefaultValue(AuthenticatedAsToolTipDefault), Localizable(true), Category(AppearanceCategory)]
		[Description("The tool tip text that appears on the green checkmark when authentication succeeds.")]
		public string AuthenticatedAsToolTip {
			get { return (string)(this.ViewState[AuthenticatedAsToolTipViewStateKey] ?? AuthenticatedAsToolTipDefault); }
			set { this.ViewState[AuthenticatedAsToolTipViewStateKey] = value ?? string.Empty; }
		}

		[PersistenceMode(PersistenceMode.InnerProperty)]
		public Collection<ProviderInfo> Providers {
			get {
				if (this.ViewState[ProvidersViewStateKey] == null) {
					var providers = new Collection<ProviderInfo>();
					this.ViewState[ProvidersViewStateKey] = providers;
					return providers;
				} else {
					return (Collection<ProviderInfo>)this.ViewState[ProvidersViewStateKey];
				}
			}
		}

		/// <summary>
		/// Gets the name of the open id auth data form key (for the value as stored at the user agent as a FORM field).
		/// </summary>
		/// <value>
		/// Usually a concatenation of the control's name and <c>"_openidAuthData"</c>.
		/// </value>
		protected override string OpenIdAuthDataFormKey {
			get { return this.ClientID + "_openidAuthData"; }
		}

		/// <summary>
		/// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
		/// </summary>
		protected override void CreateChildControls() {
			base.CreateChildControls();

			this.textBox = new OpenIdAjaxTextBox();
			this.textBox.ID = "openid_identifier";
			this.textBox.HookFormSubmit = false;
			this.Controls.Add(this.textBox);

			this.positiveAssertionField = new HiddenField();
			this.positiveAssertionField.ID = this.OpenIdAuthDataFormKey;
			this.Controls.Add(this.positiveAssertionField);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			var css = new HtmlLink();
			css.Href = this.Page.ClientScript.GetWebResourceUrl(this.GetType(), EmbeddedStylesheetResourceName);
			css.Attributes["rel"] = "stylesheet";
			css.Attributes["type"] = "text/css";
			ErrorUtilities.VerifyHost(this.Page.Header != null, OpenIdStrings.HeadTagMustIncludeRunatServer);
			this.Page.Header.Controls.AddAt(0, css); // insert at top so host page can override

			// Import the .js file where most of the code is.
			this.Page.ClientScript.RegisterClientScriptResource(typeof(OpenIdButtonPanel), EmbeddedScriptResourceName);

			// Provide javascript with a way to post the login assertion.
			const string postLoginAssertionMethodName = "postLoginAssertion";
			const string positiveAssertionParameterName = "positiveAssertion";
			string script = string.Format(
				CultureInfo.InvariantCulture,
@"window.{2} = function({0}) {{
	$('#{3}')[0].setAttribute('value', {0});
	{1};
}};",
				positiveAssertionParameterName,
				this.Page.ClientScript.GetPostBackEventReference(this, null, false),
				postLoginAssertionMethodName,
				this.positiveAssertionField.ClientID);
			this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "Postback", script, true);

			this.PreloadDiscovery(this.Providers.Select(op => op.OPIdentifier).Where(id => id != null));
		}

		/// <summary>
		/// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
		protected override void Render(HtmlTextWriter writer) {
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "OpenIdProviders");
			writer.RenderBeginTag(HtmlTextWriterTag.Ul);

			foreach (var op in this.Providers) {
				writer.AddAttribute(HtmlTextWriterAttribute.Id, (string)op.OPIdentifier ?? "OpenIDButton");
				writer.RenderBeginTag(HtmlTextWriterTag.Li);

				writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
				writer.RenderBeginTag(HtmlTextWriterTag.A);

				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				writer.RenderBeginTag(HtmlTextWriterTag.Div);

				writer.AddAttribute(HtmlTextWriterAttribute.Src, op.Image);
				writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();

				writer.AddAttribute(HtmlTextWriterAttribute.Src, this.Page.ClientScript.GetWebResourceUrl(typeof(OpenIdAjaxTextBox), OpenIdAjaxTextBox.EmbeddedLoginSuccessResourceName));
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "loginSuccess");
				writer.AddAttribute(HtmlTextWriterAttribute.Title, this.AuthenticatedAsToolTip);
				writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();

				writer.RenderEndTag(); // </div>

				writer.AddAttribute(HtmlTextWriterAttribute.Class, "ui-widget-overlay");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				writer.RenderEndTag();

				writer.RenderEndTag(); // </div>
				writer.RenderEndTag(); // </a>
				writer.RenderEndTag(); // </li>
			}

			writer.RenderEndTag(); // </ul>

			writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "OpenIDForm");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			this.RenderChildren(writer);

			writer.RenderEndTag(); // </div>
		}
	}

	public class ProviderInfo {
		public ProviderInfo() {
		}

		[TypeConverter(typeof(IdentifierConverter))]
		public Identifier OPIdentifier { get; set; }
		
		public string Image { get; set; }
	}
}
