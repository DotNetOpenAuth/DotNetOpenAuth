using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using IProviderAssociationStore = DotNetOpenId.IAssociationStore<DotNetOpenId.AssociationRelyingPartyType>;
using ProviderMemoryStore = DotNetOpenId.AssociationMemoryStore<DotNetOpenId.AssociationRelyingPartyType>;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// Offers services for a web page that is acting as an OpenID identity server.
	/// </summary>
	[DebuggerDisplay("Endpoint: {Endpoint}, OpenId Request: {Query.ContainsKey(\"openid.mode\")}")]
	public class OpenIdProvider {
		internal Signatory Signatory { get; private set; }
		internal MessageEncoder Encoder;
		/// <summary>
		/// The incoming request's Url.
		/// </summary>
		/// <remarks>
		/// This is used for certain security checks internally.  It should not
		/// be used for its Query property, as it will be irrelevant on POST requests.
		/// Instead, use the OpenIdProvider.Query field.
		/// </remarks>
		internal Uri RequestUrl;
		/// <summary>
		/// The query of the incoming request.
		/// </summary>
		internal IDictionary<string, string> Query;
		/// <summary>
		/// The version of OpenId being used by the Relying Party
		/// sending the incoming request.
		/// </summary>
		internal Protocol Protocol { get; private set; }

		/// <summary>
		/// Constructs an OpenId server that uses the HttpApplication dictionary as
		/// its association store and detects common settings.
		/// </summary>
		/// <remarks>
		/// This method requires a current ASP.NET HttpContext.
		/// </remarks>
		public OpenIdProvider()
			: this(HttpApplicationStore,
			getProviderEndpointFromContext(), Util.GetRequestUrlFromContext(), Util.GetQueryFromContext()) { }
		/// <summary>
		/// Constructs an OpenId server that uses a given query and IAssociationStore.
		/// </summary>
		/// <param name="store">
		/// The application-level store where associations with OpenId consumers will be preserved.
		/// </param>
		/// <param name="providerEndpoint">
		/// The Internet-facing URL that responds to OpenID requests.
		/// </param>
		/// <param name="requestUrl">The incoming request URL.</param>
		/// <param name="query">
		/// The name/value pairs that came in on the 
		/// QueryString of a GET request or in the entity of a POST request.
		/// For example: (Request.HttpMethod == "GET" ? Request.QueryString : Request.Form).
		/// </param>
		public OpenIdProvider(IProviderAssociationStore store, Uri providerEndpoint, Uri requestUrl, NameValueCollection query)
			: this(store, providerEndpoint, requestUrl, Util.NameValueCollectionToDictionary(query)) { }
		OpenIdProvider(IProviderAssociationStore store, Uri providerEndpoint, Uri requestUrl, IDictionary<string, string> query) {
			if (store == null) throw new ArgumentNullException("store");
			if (providerEndpoint == null) throw new ArgumentNullException("providerEndpoint");
			if (requestUrl == null) throw new ArgumentNullException("requestUrl");
			if (query == null) throw new ArgumentNullException("query");
			Endpoint = providerEndpoint;
			RequestUrl = requestUrl;
			Query = query;
			Signatory = new Signatory(store);
			Encoder = new MessageEncoder();
			store.ClearExpiredAssociations(); // every so often we should do this.
		}

		/// <summary>
		/// The provider URL that responds to OpenID requests.
		/// </summary>
		/// <remarks>
		/// An auto-detect attempt is made if an ASP.NET HttpContext is available.
		/// </remarks>
		internal Uri Endpoint { get; private set; }

		bool requestProcessed;
		Request request;
		/// <summary>
		/// Gets the incoming OpenID request if there is one, or null if none was detected.
		/// </summary>
		/// <remarks>
		/// Requests may be infrastructural to OpenID and allow auto-responses, or they may
		/// be authentication requests where the Provider site has to make decisions based
		/// on its own user database and policies.
		/// </remarks>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] // this property getter executes code
		public IRequest Request {
			get {
				if (!requestProcessed) {
					request = decodeRequest();
					requestProcessed = true;
				}
				return request;
			}
		}

		/// <summary>
		/// Decodes an incoming web request in to a <see cref="Request"/>.
		/// </summary>
		/// <returns>A Request object, or null if the given query doesn't represent an OpenId request.</returns>
		Request decodeRequest() {
			if (!Provider.Request.IsOpenIdRequest(Query)) {
				return null;
			}

			Protocol = Protocol.Detect(Query);
			Request req = Provider.Request.CreateRequest(this);

			Logger.InfoFormat("Received OpenID {0} request.{1}{2}", req.Mode, Environment.NewLine,
				Util.ToString(Query));

			return req;
		}

		/// <summary>
		/// Allows a Provider to send an identity assertion on behalf of one
		/// of its members in order to redirect the member to a relying party
		/// web site and log him/her in immediately in one uninterrupted step.
		/// </summary>
		/// <param name="relyingParty">
		/// The URL of the relying party web site.
		/// This will typically be the home page, but may be a longer URL if
		/// that Relying Party considers the scope of its realm to be more specific.
		/// The URL provided here must allow discovery of the Relying Party's
		/// XRDS document that advertises its OpenID RP endpoint.
		/// </param>
		/// <param name="claimedIdentifier">
		/// The Identifier you are asserting your member controls.
		/// </param>
		/// <param name="localIdentifier">
		/// The Identifier you know your user by internally.  This will typically
		/// be the same as <paramref name="claimedIdentifier"/>.
		/// </param>
		/// <returns>
		/// An <see cref="IResponse"/> object describing the HTTP response to send
		/// the user agent to allow the redirect with assertion to happen.
		/// </returns>
		public IResponse PrepareUnsolicitedAssertion(Realm relyingParty, 
			Identifier claimedIdentifier, Identifier localIdentifier) {
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");
			if (claimedIdentifier == null) throw new ArgumentNullException("claimedIdentifier");
			if (localIdentifier == null) throw new ArgumentNullException("localIdentifier");

			Logger.InfoFormat("Preparing unsolicited assertion for {0}", claimedIdentifier);
			return AssertionMessage.CreateUnsolicitedAssertion(this, 
				relyingParty, claimedIdentifier, localIdentifier);
		}

		const string associationStoreKey = "DotNetOpenId.Provider.OpenIdProvider.AssociationStore";
		/// <summary>
		/// The standard state storage mechanism that uses ASP.NET's HttpApplication state dictionary
		/// to store associations.
		/// </summary>
		public static IProviderAssociationStore HttpApplicationStore {
			get {
				HttpContext context = HttpContext.Current;
				if (context == null)
					throw new InvalidOperationException(Strings.IAssociationStoreRequiredWhenNoHttpContextAvailable);
				var store = (IProviderAssociationStore)context.Application[associationStoreKey];
				if (store == null) {
					context.Application.Lock();
					try {
						if ((store = (IProviderAssociationStore)context.Application[associationStoreKey]) == null) {
							context.Application[associationStoreKey] = store = new ProviderMemoryStore();
						}
					} finally {
						context.Application.UnLock();
					}
				}
				return store;
			}
		}
		static Uri getProviderEndpointFromContext() {
			HttpContext context = HttpContext.Current;
			if (context == null)
				throw new InvalidOperationException(Strings.HttpContextRequiredForThisOverload);
			UriBuilder builder = new UriBuilder(Util.GetRequestUrlFromContext());
			builder.Query = null;
			builder.Fragment = null;
			return builder.Uri;
		}
	}
}
