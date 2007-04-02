using System;
using System.Collections.Specialized;
using System.Text;
using Janrain.OpenId.Store;


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

        public Server(IAssociationStore store)
        {
  
            
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
        

    }
}
