namespace Janrain.OpenId.Store

import System
import System.Collections
import System.Security.Cryptography
import System.Web
import System.Web.Caching

import Janrain.OpenId

class MemoryStore(IAssociationStore):

    private static local_instance = MemoryStore()

    private cache as Cache = HttpRuntime.Cache

    AuthKey as (byte):
        get:
            lock self:
                auth_key = self.cache['OpenId.authKey']
                if auth_key is null:
                    auth_key = array(byte, 20)
                    RNGCryptoServiceProvider().GetBytes(auth_key)
                    self.cache.Insert('OpenId.authKey', auth_key)
                    
                return auth_key

    IsDumb as bool:
        get:
            return false

    private ServerAssocsTable as Hashtable:
        get:
            server_assocs = cast(Hashtable, self.cache['OpenId.serverAssocs'])
            if server_assocs is null:
                server_assocs = {}

            return server_assocs

        set:
            self.cache['OpenId.serverAssocs'] = value

    private Nonces as Hashtable:
        get:
            nonces = cast(Hashtable, self.cache['OpenId.nonces'])
            if nonces is null:
                nonces = {}

            return nonces

        set:
            self.cache['OpenId.nonces'] = value


    private def constructor():
        pass

    static def GetInstance():
        return local_instance

    def GetServerAssocs(server_url as Uri):
        lock self:
            table = self.ServerAssocsTable
            if not table.ContainsKey(server_url):
                table.Add(server_url, ServerAssocs())
                self.ServerAssocsTable = table
            return cast(ServerAssocs, table[server_url])

    def StoreAssociation(server_url as Uri, assoc as Association):
        lock self:
            table = self.ServerAssocsTable
            if not table.ContainsKey(server_url):
                table.Add(server_url, ServerAssocs())
            server_assocs = cast(ServerAssocs, table[server_url])
            server_assocs.Set(assoc)
            self.ServerAssocsTable = table

    def GetAssociation(serverUri as Uri):
        lock self:
            return GetServerAssocs(serverUri).Best()

    def GetAssociation(serverUri as Uri, handle as string):
        lock self:
            return GetServerAssocs(serverUri).Get(handle)

    def RemoveAssociation(serverUri as Uri, handle as string) as bool:
        lock self:
            return GetServerAssocs(serverUri).Remove(handle)

    public def StoreNonce(nonce as string):
        lock self:
            nonces = self.Nonces
            nonces[nonce] = 0
            self.Nonces = nonces

    public def UseNonce(nonce as string):
        lock self:
            nonces = self.Nonces
            present = nonces.ContainsKey(nonce)
            nonces.Remove(nonce)
            self.Nonces = nonces
            return present

    protected class ServerAssocs:
        private assocs as Hashtable

        def constructor():
            self.assocs = Hashtable()

        def Set(assoc as Association):
            self.assocs.Add(assoc.Handle, assoc)

        def Get(handle as string):
            assoc as Association = null
            if self.assocs.Contains(handle):
                assoc = cast(Association, self.assocs[handle])
            return assoc

        def Remove(handle as string):
            present = self.assocs.Contains(handle)
            self.assocs.Remove(handle)
            return present

        def Best():
            best as Association = null
            for assoc as Association in self.assocs.Values:
                if (best is null) or (best.Issued < assoc.Issued):
                    best = assoc
            return best

