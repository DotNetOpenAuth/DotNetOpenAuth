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
		internal IDictionary<string, string> query;

		/// <summary>
		/// Constructs an OpenId server that uses the HttpApplication dictionary as
		/// its association store.
		/// </summary>
		/// <remarks>
		/// This method requires a current ASP.NET HttpContext.
		/// </remarks>
		public OpenIdProvider() : this(Util.GetQueryFromContext(), httpApplicationAssociationStore) { }
		/// <summary>
		/// Constructs an OpenId server that uses a given query and IAssociationStore.
		/// </summary>
		/// <param name="query">The name/value pairs that came in on the QueryString of the web request.</param>
		/// <param name="store">
		/// The application-level store where associations with OpenId consumers will be preserved.
		/// </param>
		public OpenIdProvider(NameValueCollection query, IProviderAssociationStore store)
			: this (Util.NameValueCollectionToDictionary(query), store) {}
		OpenIdProvider(IDictionary<string, string> query, IProviderAssociationStore store) {
			if (query == null) throw new ArgumentNullException("query");
			if (store == null) throw new ArgumentNullException("store");
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
		/// <returns>A Request object, or null if the given query doesn't represent an OpenId request.</returns>
		Request decodeRequest() {
			if (!Provider.Request.IsOpenIdRequest(query)) {
				return null;
			}

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Start message decoding");
			}

			Request req = Provider.Request.CreateRequest(this, query);

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("End message decoding. Successfully decoded message as new {0}.", req.GetType().Name);
				Trace.TraceInformation(req.ToString());
			}

			return req;
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
