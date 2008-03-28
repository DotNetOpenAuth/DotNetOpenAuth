using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using IProviderAssociationStore = DotNetOpenId.IAssociationStore<DotNetOpenId.AssociationRelyingPartyType>;
using ProviderMemoryStore = DotNetOpenId.AssociationMemoryStore<DotNetOpenId.AssociationRelyingPartyType>;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// Offers services for a web page that is acting as an OpenID identity server.
	/// </summary>
	public class OpenIdProvider {
		internal Signatory Signatory { get; private set; }
		internal Encoder Encoder;
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
		/// its association store.
		/// </summary>
		/// <remarks>
		/// This method requires a current ASP.NET HttpContext.
		/// </remarks>
		public OpenIdProvider()
			: this(httpApplicationAssociationStore,
			Util.GetRequestUrlFromContext(), Util.GetQueryFromContext()) { }
		/// <summary>
		/// Constructs an OpenId server that uses a given query and IAssociationStore.
		/// </summary>
		/// <param name="store">
		/// The application-level store where associations with OpenId consumers will be preserved.
		/// </param>
		/// <param name="requestUrl">The incoming request URL.</param>
		/// <param name="query">The name/value pairs that came in on the 
		/// QueryString of a GET request or in the entity of a POST request.</param>
		public OpenIdProvider(IProviderAssociationStore store, Uri requestUrl, NameValueCollection query)
			: this(store, requestUrl, Util.NameValueCollectionToDictionary(query)) { }
		OpenIdProvider(IProviderAssociationStore store, Uri requestUrl, IDictionary<string, string> query) {
			if (store == null) throw new ArgumentNullException("store");
			if (requestUrl == null) throw new ArgumentNullException("requestUrl");
			if (query == null) throw new ArgumentNullException("query");
			RequestUrl = requestUrl;
			Query = query;
			Signatory = new Signatory(store);
			Encoder = new SigningEncoder(Signatory);
			store.ClearExpiredAssociations(); // every so often we should do this.
		}

		bool requestProcessed;
		Request request;
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

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Start message decoding");
			}

			Protocol = Protocol.Detect(Query);
			Request req = Provider.Request.CreateRequest(this);

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("End message decoding. Successfully decoded message as new {0}.", req.GetType().Name);
				Trace.TraceInformation(req.ToString());
			}

			return req;
		}

		const string associationStoreKey = "DotNetOpenId.Provider.OpenIdProvider.AssociationStore";
		static IProviderAssociationStore httpApplicationAssociationStore {
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
	}
}
