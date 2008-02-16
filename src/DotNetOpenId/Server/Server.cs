using System;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId.Store;
using System.Web;


namespace DotNetOpenId.Server {
	/// <summary>
	/// Offers services for a web page that is acting as an OpenID identity server.
	/// </summary>
	public class Server {
		IAssociationStore store;
		Signatory signatory;
		Encoder encoder;

		/// <summary>
		/// Constructs an OpenId server that uses the HttpApplication dictionary as
		/// its association store.
		/// </summary>
		public Server() : this(httpApplicationAssociationStore) { }

		/// <summary>
		/// Constructs an OpenId server that uses a given IAssociationStore.
		/// </summary>
		public Server(IAssociationStore store) {
			if (store == null) throw new ArgumentNullException("store");
			this.store = store;
			this.signatory = new Signatory(store);
			this.encoder = new SigningEncoder(signatory);
		}

		public IEncodable HandleRequest(Request request) {
			Response response;
			switch (request.RequestType) {
				case RequestType.CheckAuthRequest:
					response = ((CheckAuthRequest)request).Answer(signatory);
					break;
				case RequestType.AssociateRequest:
					response = ((AssociateRequest)request).Answer(signatory.CreateAssociation(false));
					break;
				case RequestType.CheckIdRequest:
				default:
					throw new ArgumentException("Unexpected Request.RequestMode value.", "request");
			}
			return response;
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

		/// <returns>
		/// Null if the given HttpRequest does not represent a request from an 
		/// OpenId client.  This could occur if someone just typed in an OpenID
		/// URL directly.
		/// </returns>
		public Request DecodeRequest(NameValueCollection query) {
			return Request.GetRequestFromQuery(query);
		}

		public WebResponse EncodeResponse(IEncodable response) {
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace("Encoding response");
			}

			return this.encoder.Encode(response);
		}

		const string associationStoreKey = "DotNetOpenId.Server.Server.AssociationStore";
		static IAssociationStore httpApplicationAssociationStore {
			get {
				HttpContext context = HttpContext.Current;
				if (context == null)
					throw new InvalidOperationException(Strings.IAssociationStoreRequiredWhenNoHttpContextAvailable);
				IAssociationStore store = (IAssociationStore)context.Application[associationStoreKey];
				if (store == null) {
					context.Application.Lock();
					try {
						if ((store = (IAssociationStore)context.Application[associationStoreKey]) == null) {
							context.Application[associationStoreKey] = store = new MemoryStore();
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
