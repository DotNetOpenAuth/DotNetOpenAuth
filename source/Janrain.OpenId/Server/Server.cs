using System;
using System.Collections.Specialized;
using System.Text;
using Janrain.OpenId.Store;
using System.Web;


namespace Janrain.OpenId.Server
{
    public class Server
    {

        #region Private Members

        private IAssociationStore _store;
        private Signatory _signatory;
        private Encoder _encoder;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Constructs an OpenId server that uses the HttpApplication dictionary as
        /// its association store.
        /// </summary>
        public Server() : this(HttpApplicationAssociationStore) { }

        /// <summary>
        /// Constructs an OpenId server that uses a given IAssociationStore.
        /// </summary>
        public Server(IAssociationStore store)
        {
            if (store == null) throw new ArgumentNullException("store");
            _store = store;
            _signatory = new Signatory(store);
            _encoder = new SigningEncoder(_signatory);
        }

        #endregion

        #region Methods

        public Response HandleRequest(CheckAuthRequest request)        
        {
            Response response =  request.Answer(_signatory);
            return response;
            
        }

        public Response HandleRequest(AssociateRequest request)
        {
            Association assoc = _signatory.CreateAssociation(false);
            Response response = request.Answer(assoc);
            return response;
        }

        #endregion

        public Request DecodeRequest(NameValueCollection query)
        {
            return Decoder.Decode(query);
        }

        public WebResponse EncodeResponse(IEncodable response)
        {
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("Encoding response");
            }
            #endregion
            
            return this._encoder.Encode(response);
        }

        const string associationStoreKey = "Janrain.OpenId.Server.Server.AssociationStore";
        static IAssociationStore HttpApplicationAssociationStore {
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
