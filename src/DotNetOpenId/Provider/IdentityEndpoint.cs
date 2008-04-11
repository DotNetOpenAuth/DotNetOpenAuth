using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// An ASP.NET control that manages the OpenID identity advertising tags
	/// of a user's Identity Page that allow a relying party web site to discover
	/// how to authenticate a user.
	/// </summary>
	[DefaultProperty("ServerUrl")]
	[ToolboxData("<{0}:IdentityEndpoint runat=server></{0}:IdentityEndpoint>")]
	public class IdentityEndpoint : XrdsPublisher {

		#region Properties
		const string providerVersionViewStateKey = "ProviderVersion";
		const ProtocolVersion providerVersionDefault = ProtocolVersion.V20;
		/// <summary>
		/// The OpenID version supported by the provider.
		/// If multiple versions are supported, this should be set to the latest
		/// version that DotNetOpenId and the Provider both support.
		/// </summary>
		[Category("Behavior")]
		[DefaultValue(providerVersionDefault)]
		public ProtocolVersion ProviderVersion {
			get {
				return ViewState[providerVersionViewStateKey] == null ?
				providerVersionDefault : (ProtocolVersion)ViewState[providerVersionViewStateKey];
			}
			set { ViewState[providerVersionViewStateKey] = value; }
		}

		const string providerEndpointUrlViewStateKey = "ProviderEndpointUrl";
		/// <summary>
		/// The Provider URL that processes OpenID requests.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), Bindable(true)]
		[Category("Behavior")]
		public string ProviderEndpointUrl {
			get { return (string)ViewState[providerEndpointUrlViewStateKey]; }
			set {
				RelyingParty.OpenIdTextBox.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[providerEndpointUrlViewStateKey] = value;
			}
		}

		const string providerLocalIdentifierViewStateKey = "ProviderLocalIdentifier";
		/// <summary>
		/// The Identifier that is controlled by the Provider.
		/// </summary>
		[Bindable(true)]
		[Category("Behavior")]
		public string ProviderLocalIdentifier {
			get { return (string)ViewState[providerLocalIdentifierViewStateKey]; }
			set {
				RelyingParty.OpenIdTextBox.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[providerLocalIdentifierViewStateKey] = value;
			}
		}
		#endregion

		internal Protocol Protocol {
			get { return Protocol.Lookup(ProviderVersion); }
		}

		/// <summary>
		/// Renders OpenID identity tags.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		protected override void Render(HtmlTextWriter writer) {
			base.Render(writer);
			if (!string.IsNullOrEmpty(ProviderEndpointUrl)) {
				writer.WriteBeginTag("link");
				writer.WriteAttribute("rel", Protocol.HtmlDiscoveryProviderKey);
				writer.WriteAttribute("href",
					new Uri(Page.Request.Url, Page.ResolveUrl(ProviderEndpointUrl)).AbsoluteUri);
				writer.Write(">");
				writer.WriteEndTag("link");
				writer.WriteLine();
			}
			if (!string.IsNullOrEmpty(ProviderLocalIdentifier)) {
				writer.WriteBeginTag("link");
				writer.WriteAttribute("rel", Protocol.HtmlDiscoveryLocalIdKey);
				writer.WriteAttribute("href",
					new Uri(Page.Request.Url, Page.ResolveUrl(ProviderLocalIdentifier)).AbsoluteUri);
				writer.Write(">");
				writer.WriteEndTag("link");
				writer.WriteLine();
			}
		}
	}
}
