using System;
using System.ComponentModel;
using System.Web.UI;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// The event arguments passed to the <see cref="IdentityEndpoint.NormalizeUri"/> event handler.
	/// </summary>
	public class IdentityEndpointNormalizationEventArgs : EventArgs {
		internal IdentityEndpointNormalizationEventArgs(UriIdentifier userSuppliedIdentifier) {
			UserSuppliedIdentifier = userSuppliedIdentifier;
		}

		/// <summary>
		/// Gets the portion of the incoming page request URI that is relevant to normalization.
		/// </summary>
		/// <remarks>
		/// This identifier should be used to look up the user whose identity page is being queried.
		/// </remarks>
		public Uri UserSuppliedIdentifier { get; private set; }

		/// <summary>
		/// Gets/sets the normalized form of the user's identifier, according to the host site's policy.
		/// </summary>
		/// <remarks>
		/// <para>This should be set to some constant value for an individual user.  
		/// For example, if <see cref="UserSuppliedIdentifier"/> indicates that identity page
		/// for "BOB" is being called up, then the following things should be considered:</para>
		/// <list>
		/// <item>Normalize the capitalization of the URL: for example, change http://provider/BOB to
		/// http://provider/bob.</item>
		/// <item>Switch to HTTPS is it is offered: change http://provider/bob to https://provider/bob.</item>
		/// <item>Strip off the query string if it is not part of the canonical identity:
		/// https://provider/bob?timeofday=now becomes https://provider/bob</item>
		/// <item>Ensure that any trailing slash is either present or absent consistently.  For example,
		/// change https://provider/bob/ to https://provider/bob.</item>
		/// </list>
		/// <para>When this property is set, the <see cref="IdentityEndpoint"/> control compares it to
		/// the request that actually came in, and redirects the browser to use the normalized identifier
		/// if necessary.</para>
		/// <para>Using the normalized identifier in the request is <i>very</i> important as it
		/// helps the user maintain a consistent identity across sites and across site visits to an individual site.
		/// For example, without normalizing the URL, Bob might sign into a relying party site as 
		/// http://provider/bob one day and https://provider/bob the next day, and the relying party
		/// site <i>should</i> interpret Bob as two different people because the URLs are different.
		/// By normalizing the URL at the Provider's identity page for Bob, whichever URL Bob types in
		/// from day-to-day gets redirected to a normalized form, so Bob is seen as the same person
		/// all the time, which is of course what Bob wants.
		/// </para>
		/// </remarks>
		public Uri NormalizedIdentifier { get; set; }
	}

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
		[Description("The OpenID version supported by the provider.")]
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
		[Description("The Provider URL that processes OpenID requests.")]
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
		[Description("The user Identifier that is controlled by the Provider.")]
		public string ProviderLocalIdentifier {
			get { return (string)ViewState[providerLocalIdentifierViewStateKey]; }
			set {
				RelyingParty.OpenIdTextBox.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[providerLocalIdentifierViewStateKey] = value;
			}
		}

		const string autoNormalizeRequestViewStateKey = "AutoNormalizeRequest";
		/// <summary>
		/// Whether every incoming request will be checked for normalized form and redirected if it is not.
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
			get { return (bool)(ViewState[autoNormalizeRequestViewStateKey] ?? false); }
			set { ViewState[autoNormalizeRequestViewStateKey] = value; }
		}
		#endregion

		internal Protocol Protocol {
			get { return Protocol.Lookup(ProviderVersion); }
		}

		/// <summary>
		/// Fired at each page request so the host web site can return the normalized
		/// version of the request URI.
		/// </summary>
		public event EventHandler<IdentityEndpointNormalizationEventArgs> NormalizeUri;

		/// <summary>
		/// Checks the incoming request and invokes a browser redirect if the URL has not been normalized.
		/// </summary>
		/// <seealso cref="IdentityEndpointNormalizationEventArgs.NormalizedIdentifier"/>
		protected virtual void OnNormalize() {
			UriIdentifier userSuppliedIdentifier = Util.GetRequestUrlFromContext();
			var normalizationArgs = new IdentityEndpointNormalizationEventArgs(userSuppliedIdentifier);

			var normalizeUri = NormalizeUri;
			if (normalizeUri != null) {
				normalizeUri(this, normalizationArgs);
			} else {
				// Do some best-guess normalization.
				normalizationArgs.NormalizedIdentifier = bestGuessNormalization(normalizationArgs.UserSuppliedIdentifier);
			}
			// If we have a normalized form, we should use it.
			// We only compare path and query because the host name SHOULD NOT be case sensitive,
			// and the fragment will be asserted or cleared by the OP during authentication.
			if (normalizationArgs.NormalizedIdentifier != null &&
				!String.Equals(normalizationArgs.NormalizedIdentifier.PathAndQuery, userSuppliedIdentifier.Uri.PathAndQuery, StringComparison.Ordinal)) {
				Page.Response.Redirect(normalizationArgs.NormalizedIdentifier.AbsoluteUri);
			}
		}

		/// <summary>
		/// Normalizes the URL by making the path and query lowercase, and trimming trailing slashes.
		/// </summary>
		private static Uri bestGuessNormalization(Uri uri) {
			UriBuilder uriBuilder = new UriBuilder(uri);
			uriBuilder.Path = uriBuilder.Path.ToLowerInvariant();
			// Ensure no trailing slash unless it is the only element of the path.
			if (uriBuilder.Path != "/") {
				uriBuilder.Path = uriBuilder.Path.TrimEnd('/');
			}
			// We trim the ? from the start of the query when we reset it because
			// the UriBuilder.Query setter automatically prepends one, and we don't
			// want to double them up.
			uriBuilder.Query = uriBuilder.Query.TrimStart('?').ToLower();
			return uriBuilder.Uri;
		}

		/// <summary>
		/// Checks the incoming request and invokes a browser redirect if the URL has not been normalized.
		/// </summary>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (AutoNormalizeRequest && !Page.IsPostBack) {
				OnNormalize();
			}
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
					new Uri(Util.GetRequestUrlFromContext(), Page.ResolveUrl(ProviderEndpointUrl)).AbsoluteUri);
				writer.Write(">");
				writer.WriteEndTag("link");
				writer.WriteLine();
			}
			if (!string.IsNullOrEmpty(ProviderLocalIdentifier)) {
				writer.WriteBeginTag("link");
				writer.WriteAttribute("rel", Protocol.HtmlDiscoveryLocalIdKey);
				writer.WriteAttribute("href",
					new Uri(Util.GetRequestUrlFromContext(), Page.ResolveUrl(ProviderLocalIdentifier)).AbsoluteUri);
				writer.Write(">");
				writer.WriteEndTag("link");
				writer.WriteLine();
			}
		}
	}
}
