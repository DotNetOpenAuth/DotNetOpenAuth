//-----------------------------------------------------------------------
// <copyright file="IdentityEndpoint.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Drawing.Design;
	using System.Web.UI;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An ASP.NET control that manages the OpenID identity advertising tags
	/// of a user's Identity Page that allow a relying party web site to discover
	/// how to authenticate a user.
	/// </summary>
	[DefaultProperty("ServerUrl")]
	[ToolboxData("<{0}:IdentityEndpoint runat=\"server\" ProviderEndpointUrl=\"\" />")]
	public class IdentityEndpoint : XrdsPublisher {
		#region Property viewstate keys

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="AutoNormalizeRequest"/> property.
		/// </summary>
		private const string AutoNormalizeRequestViewStateKey = "AutoNormalizeRequest";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="ProviderLocalIdentifier"/> property.
		/// </summary>
		private const string ProviderLocalIdentifierViewStateKey = "ProviderLocalIdentifier";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="ProviderVersion"/> property.
		/// </summary>
		private const string ProviderVersionViewStateKey = "ProviderVersion";

		/// <summary>
		/// The viewstate key to use for storing the value of the <see cref="ProviderEndpointUrl"/> property.
		/// </summary>
		private const string ProviderEndpointUrlViewStateKey = "ProviderEndpointUrl";

		#endregion

		/// <summary>
		/// The default value for the <see cref="ProviderVersion"/> property.
		/// </summary>
		private const ProtocolVersion ProviderVersionDefault = ProtocolVersion.V20;

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentityEndpoint"/> class.
		/// </summary>
		public IdentityEndpoint() {
		}

		/// <summary>
		/// Fired at each page request so the host web site can return the normalized
		/// version of the request URI.
		/// </summary>
		public event EventHandler<IdentityEndpointNormalizationEventArgs> NormalizeUri;

		#region Properties

		/// <summary>
		/// Gets or sets the OpenID version supported by the provider.
		/// If multiple versions are supported, this should be set to the latest
		/// version that this library and the Provider both support.
		/// </summary>
		[Category("Behavior")]
		[DefaultValue(ProviderVersionDefault)]
		[Description("The OpenID version supported by the provider.")]
		public ProtocolVersion ProviderVersion {
			get {
				return this.ViewState[ProviderVersionViewStateKey] == null ?
				ProviderVersionDefault : (ProtocolVersion)this.ViewState[ProviderVersionViewStateKey];
			}

			set {
				this.ViewState[ProviderVersionViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets the Provider URL that processes OpenID requests.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Forms designer property grid only supports primitive types.")]
		[Bindable(true), Category("Behavior")]
		[Description("The Provider URL that processes OpenID requests.")]
		[UrlProperty, Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		public string ProviderEndpointUrl {
			get {
				return (string)ViewState[ProviderEndpointUrlViewStateKey];
			}

			set {
				UriUtil.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[ProviderEndpointUrlViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets the Identifier that is controlled by the Provider.
		/// </summary>
		[Bindable(true)]
		[Category("Behavior")]
		[Description("The user Identifier that is controlled by the Provider.")]
		public string ProviderLocalIdentifier {
			get {
				return (string)ViewState[ProviderLocalIdentifierViewStateKey];
			}

			set {
				UriUtil.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[ProviderLocalIdentifierViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether every incoming request 
		/// will be checked for normalized form and redirected if it is not.
		/// </summary>
		/// <remarks>
		/// <para>If set to true (and it should be), you should also handle the <see cref="NormalizeUri"/>
		/// event and apply your own policy for normalizing the URI.</para>
		/// If multiple <see cref="IdentityEndpoint"/> controls are on a single page (to support
		/// multiple versions of OpenID for example) then only one of them should have this 
		/// property set to true.
		/// </remarks>
		[Bindable(true)]
		[Category("Behavior")]
		[Description("Whether every incoming request will be checked for normalized form and redirected if it is not.  If set to true, consider handling the NormalizeUri event.")]
		public bool AutoNormalizeRequest {
			get { return (bool)(ViewState[AutoNormalizeRequestViewStateKey] ?? false); }
			set { ViewState[AutoNormalizeRequestViewStateKey] = value; }
		}
		#endregion

		/// <summary>
		/// Gets the protocol to use for advertising OpenID on the identity page.
		/// </summary>
		internal Protocol Protocol {
			get { return Protocol.Lookup(this.ProviderVersion); }
		}

		/// <summary>
		/// Checks the incoming request and invokes a browser redirect if the URL has not been normalized.
		/// </summary>
		/// <seealso cref="IdentityEndpointNormalizationEventArgs.NormalizedIdentifier"/>
		protected virtual void OnNormalize() {
			UriIdentifier userSuppliedIdentifier = MessagingUtilities.GetRequestUrlFromContext();
			var normalizationArgs = new IdentityEndpointNormalizationEventArgs(userSuppliedIdentifier);

			var normalizeUri = this.NormalizeUri;
			if (normalizeUri != null) {
				normalizeUri(this, normalizationArgs);
			} else {
				// Do some best-guess normalization.
				normalizationArgs.NormalizedIdentifier = BestGuessNormalization(normalizationArgs.UserSuppliedIdentifier);
			}

			// If we have a normalized form, we should use it.
			// We compare path and query with case sensitivity and host name without case sensitivity deliberately,
			// and the fragment will be asserted or cleared by the OP during authentication.
			if (normalizationArgs.NormalizedIdentifier != null &&
				(!string.Equals(normalizationArgs.NormalizedIdentifier.Host, normalizationArgs.UserSuppliedIdentifier.Host, StringComparison.OrdinalIgnoreCase) ||
				!string.Equals(normalizationArgs.NormalizedIdentifier.PathAndQuery, normalizationArgs.UserSuppliedIdentifier.PathAndQuery, StringComparison.Ordinal))) {
				Page.Response.Redirect(normalizationArgs.NormalizedIdentifier.AbsoluteUri);
			}
		}

		/// <summary>
		/// Checks the incoming request and invokes a browser redirect if the URL has not been normalized.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnLoad(EventArgs e) {
			// Perform URL normalization BEFORE calling base.OnLoad, to keep
			// our base XrdsPublisher from over-eagerly responding with an XRDS
			// document before we've redirected.
			if (this.AutoNormalizeRequest && !this.Page.IsPostBack) {
				this.OnNormalize();
			}

			base.OnLoad(e);
		}

		/// <summary>
		/// Renders OpenID identity tags.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Uri(Uri, string) accepts second arguments that Uri(Uri, new Uri(string)) does not that we must support.")]
		protected override void Render(HtmlTextWriter writer) {
			Uri requestUrlBeforeRewrites = MessagingUtilities.GetRequestUrlFromContext();
			base.Render(writer);
			if (!string.IsNullOrEmpty(this.ProviderEndpointUrl)) {
				writer.WriteBeginTag("link");
				writer.WriteAttribute("rel", this.Protocol.HtmlDiscoveryProviderKey);
				writer.WriteAttribute("href", new Uri(requestUrlBeforeRewrites, this.Page.Response.ApplyAppPathModifier(this.ProviderEndpointUrl)).AbsoluteUri);
				writer.Write(">");
				writer.WriteEndTag("link");
				writer.WriteLine();
			}
			if (!string.IsNullOrEmpty(this.ProviderLocalIdentifier)) {
				writer.WriteBeginTag("link");
				writer.WriteAttribute("rel", Protocol.HtmlDiscoveryLocalIdKey);
				writer.WriteAttribute("href", new Uri(requestUrlBeforeRewrites, this.Page.Response.ApplyAppPathModifier(this.ProviderLocalIdentifier)).AbsoluteUri);
				writer.Write(">");
				writer.WriteEndTag("link");
				writer.WriteLine();
			}
		}

		/// <summary>
		/// Normalizes the URL by making the path and query lowercase, and trimming trailing slashes.
		/// </summary>
		/// <param name="uri">The URI to normalize.</param>
		/// <returns>The normalized URI.</returns>
		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "FxCop is probably right, but we've been lowercasing host names for normalization elsewhere in the project for a long time now.")]
		private static Uri BestGuessNormalization(Uri uri) {
			UriBuilder uriBuilder = new UriBuilder(uri);
			uriBuilder.Path = uriBuilder.Path.ToLowerInvariant();

			// Ensure no trailing slash unless it is the only element of the path.
			if (uriBuilder.Path != "/") {
				uriBuilder.Path = uriBuilder.Path.TrimEnd('/');
			}

			// We trim the ? from the start of the query when we reset it because
			// the UriBuilder.Query setter automatically prepends one, and we don't
			// want to double them up.
			uriBuilder.Query = uriBuilder.Query.TrimStart('?').ToLowerInvariant();
			return uriBuilder.Uri;
		}
	}
}
