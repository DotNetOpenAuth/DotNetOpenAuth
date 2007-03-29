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
            return request.Answer(_signatory);
        }

        public Response HandleRequest(AssociateRequest request)
        {
            Association assoc = _signatory.CreateAssociation(false);
            return request.Answer(assoc);
        }

        #endregion

        public Request DecodeRequest(NameValueCollection query)
        {
            return Decoder.Decode(query);
        }

        public WebResponse EncodeResponse(IEncodable response)
        {
            return this._encoder.Encode(response);
        }
        

    }
}
