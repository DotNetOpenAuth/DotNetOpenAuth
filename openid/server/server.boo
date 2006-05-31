namespace Janrain.OpenId.Server

import System.Collections.Specialized
import Janrain.OpenId.Store

class Server:
    store as IAssociationStore
    signatory as Signatory
    encoder as Encoder

    def constructor(store as IAssociationStore):
        self.store = store
        self.signatory = Signatory(self.store)
        self.encoder = SigningEncoder(self.signatory)

    def HandleRequest(request as CheckAuthRequest):
        return request.Answer(self.signatory)

    def HandleRequest(request as AssociateRequest):
        assoc = self.signatory.CreateAssociation(false)
        return request.Answer(assoc)

    def DecodeRequest(query as NameValueCollection):
        return Decoder.Decode(query)

    def EncodeResponse(response as IEncodable):
        return self.encoder.Encode(response)




