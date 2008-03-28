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
		Encoder encoder;
		internal NameValueCollection query;

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
		public OpenIdProvider(IProviderAssociationStore store, Uri requestUrl, NameValueCollection query) {
			if (store == null) throw new ArgumentNullException("store");
			if (requestUrl == null) throw new ArgumentNullException("requestUrl");
			if (query == null) throw new ArgumentNullException("query");
			this.query = query;
			Signatory = new Signatory(store);
			this.encoder = new SigningEncoder(Signatory);
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
		/// <param name="query">The query parameters as a dictionary with each key mapping to one value. </param>
		/// <returns>A Request object, or null if the given query doesn't represent an OpenId request.</returns>
		Request decodeRequest() {
			if (!Provider.Request.IsOpenIdRequest(query)) {
				return null;
			}

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Start message decoding");
			}

			Request request = Provider.Request.CreateRequest(this, query);

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("End message decoding. Successfully decoded message as new {0}.", request.GetType().Name);
				Trace.TraceInformation(request.ToString());
			}

			return request;
		}

		internal Response EncodeResponse(IEncodable response) {
			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Encoding response");
			}

			return encoder.Encode(response);
		}

		const string associationStoreKey = "DotNetOpenId.Provider.Server.AssociationStore";
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
