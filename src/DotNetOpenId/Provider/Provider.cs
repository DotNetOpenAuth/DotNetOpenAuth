using System;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId.Store;
using System.Web;
using IProviderAssociationStore = DotNetOpenId.Store.IAssociationStore<DotNetOpenId.Store.AssociationConsumerType>;
using ProviderMemoryStore = DotNetOpenId.Store.AssociationMemoryStore<DotNetOpenId.Store.AssociationConsumerType>;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// Offers services for a web page that is acting as an OpenID identity server.
	/// </summary>
	public class Provider {
		IProviderAssociationStore store;
		internal Signatory Signatory { get; private set; }
		Encoder encoder;

		/// <summary>
		/// Constructs an OpenId server that uses the HttpApplication dictionary as
		/// its association store.
		/// </summary>
		public Provider() : this(httpApplicationAssociationStore) { }

		/// <summary>
		/// Constructs an OpenId server that uses a given IAssociationStore.
		/// </summary>
		public Provider(IProviderAssociationStore store) {
			if (store == null) throw new ArgumentNullException("store");
			this.store = store;
			Signatory = new Signatory(store);
			this.encoder = new SigningEncoder(Signatory);
		}

		/// <summary>
		/// Decodes an incoming web request in to a <see cref="Request"/>.
		/// </summary>
		/// <param name="query">The query parameters as a dictionary with each key mapping to one value. </param>
		/// <returns>A Request object, or null if the given query doesn't represent an OpenId request.</returns>
		public Request DecodeRequest(NameValueCollection query) {
			if (query == null) throw new ArgumentNullException("query");

			if (!Request.IsOpenIdRequest(query)) {
				return null;
			}

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Start message decoding");
			}

			Request request = Request.CreateRequest(this, query);

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("End message decoding. Successfully decoded message as new {0}.", request.GetType().Name);
				Trace.TraceInformation(request.ToString());
			}

			return request;
		}

		/// <returns>
		/// Null if the given HttpRequest does not represent a request from an 
		/// OpenId client.  This could occur if someone just typed in an OpenID
		/// URL directly.
		/// </returns>
		public Request DecodeRequest(HttpRequest request) {
			return DecodeRequest(
				request.HttpMethod == "GET" ? request.QueryString : request.Form);
		}

		public WebResponse HandleRequest(Request request) {
			WebResponse response;
			switch (request.RequestType) {
				case RequestType.CheckAuthRequest:
					response = EncodeResponse(((CheckAuthRequest)request).Answer());
					break;
				case RequestType.AssociateRequest:
					response = EncodeResponse(((AssociateRequest)request).Answer());
					break;
				case RequestType.CheckIdRequest:
				default:
					throw new ArgumentException("Unexpected Request.RequestMode value.  Use CheckIdRequest.Answer instead.", "request");
			}
			return response;
		}

		internal WebResponse EncodeResponse(IEncodable response) {
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
