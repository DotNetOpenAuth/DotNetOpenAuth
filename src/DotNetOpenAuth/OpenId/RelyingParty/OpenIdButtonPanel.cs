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

		private const string ProvidersViewStateKey = "Providers";

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

				writer.RenderEndTag(); // </div>

				writer.AddAttribute(HtmlTextWriterAttribute.Class, "ui-widget-overlay");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				writer.RenderEndTag();

				writer.RenderEndTag(); // </div>
				writer.RenderEndTag(); // </a>
				writer.RenderEndTag(); // </li>
			}

			writer.RenderEndTag(); // </ul>
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
	}

	public class ProviderInfo {
		public ProviderInfo() {
		}

		public ProviderInfo(Identifier identifier, string image) {
			this.OPIdentifier = identifier;
			this.Image = image;
		}

		[TypeConverter(typeof(IdentifierConverter))]
		public Identifier OPIdentifier { get; set; }
		
		public string Image { get; set; }
	}
}
