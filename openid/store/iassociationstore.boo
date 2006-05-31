namespace Janrain.OpenId.Store

import System
import Janrain.OpenId

interface IAssociationStore:
    AuthKey as (byte):
        get:
            pass

    IsDumb as bool:
        get:
            pass

    def StoreAssociation(serverUri as Uri, assoc as Association)

    def GetAssociation(serverUri as Uri) as Association

    def GetAssociation(serverUri as Uri, handle as string) as Association

    def RemoveAssociation(serverUri as Uri, handle as string) as bool

    def StoreNonce(nonce as string)

    def UseNonce(nonce as string) as bool

