using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Web;
using System.Web.Caching;
using Janrain.OpenId;


namespace Janrain.OpenId.Store
{

    public class MemoryStore : IAssociationStore
    {

        #region Member Variables

        private static MemoryStore local_instance = new MemoryStore();
        private Cache cache = HttpRuntime.Cache;

        #endregion

        #region Constructor(s)

        private MemoryStore() { }

        #endregion

        #region Private Properties

        private Hashtable ServerAssocsTable
        {
            get
            {
                Hashtable server_assocs = (Hashtable)this.cache["OpenId.serverAssocs"];

                if (server_assocs == null)
                    server_assocs = new Hashtable();

                return server_assocs;
            }
            set
            {
                this.cache["OpenId.serverAssocs"] = value;
            }
        }

        private Hashtable Nonces
        {
            get
            {
                Hashtable nonces = (Hashtable)this.cache["OpenId.nonces"];

                if (nonces == null)
                    nonces = new Hashtable();

                return nonces;
            }
            set
            {
                this.cache["OpenId.nonces"] = value;
            }
        }

        #endregion

        public static MemoryStore GetInstance()
        {
            return local_instance;
        }

        public ServerAssocs GetServerAssocs(Uri server_url)
        {
            lock (this)
            {
                Hashtable table = this.ServerAssocsTable;

                if (!table.ContainsKey(server_url))
                {
                    table.Add(server_url, new ServerAssocs());
                    this.ServerAssocsTable = table;
                }

                return (ServerAssocs)table[server_url];
            }
        }

        public void StoreAssociation(Uri server_url, Association assoc)
        {
            lock (this)
            {
                Hashtable table = this.ServerAssocsTable;

                if (!table.ContainsKey(server_url))
                    table.Add(server_url, new ServerAssocs());

                ServerAssocs server_assocs = (ServerAssocs)table[server_url];
                
                server_assocs.Set(assoc);

                this.ServerAssocsTable = table;
            }
        }

        public Association GetAssociation(Uri serverUri)
        {
            lock (this)
            {
                return GetServerAssocs(serverUri).Best();
            }
        }

        public Association GetAssociation(Uri serverUri, string handle)
        {
            lock (this)
            {
                return GetServerAssocs(serverUri).Get(handle);
            }
        }

        public bool RemoveAssociation(Uri serverUri, string handle)
        {
            lock (this)
            {
                return GetServerAssocs(serverUri).Remove(handle);
            }
        }

        public bool StoreNonce(string nonce)
        {
            lock (this)
            {
                Hashtable nonces = this.Nonces;
                bool present = nonces.ContainsKey(nonce);

                nonces.Remove(nonce);
                this.Nonces = nonces;

                return present;

            }
        }

        public bool UseNonce(string nonce)
        {
            lock (this)
            {
                Hashtable nonces = this.Nonces;
                bool present = nonces.ContainsKey(nonce);

                nonces.Remove(nonce);
                this.Nonces = nonces;

                return present;

            }
        }

        #region IAssociationStore Members

        byte[] IAssociationStore.AuthKey
        {
            get
            {
                lock(this) 
                {
                    byte[] auth_key = (byte[]) this.cache["OpenId.authKey"];

                    if (auth_key == null)
                    {
                        auth_key = new byte[20];
                        new RNGCryptoServiceProvider().GetBytes(auth_key);
                        this.cache.Insert("OpenId.authKey", auth_key);
                    }
                    return auth_key;
                }
            }
        }

        bool IAssociationStore.IsDumb
        {
            get { return false; }
        }

        void IAssociationStore.StoreAssociation(Uri serverUri, Association assoc)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        Association IAssociationStore.GetAssociation(Uri serverUri)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        Association IAssociationStore.GetAssociation(Uri serverUri, string handle)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        bool IAssociationStore.RemoveAssociation(Uri serverUri, string handle)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void IAssociationStore.StoreNonce(string nonce)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        bool IAssociationStore.UseNonce(string nonce)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

    public class ServerAssocs
    {
        private Hashtable assocs;

        public ServerAssocs()
        {
            this.assocs = new Hashtable();
        }

        public void Set(Association assoc)
        {
            this.assocs.Add(assoc.Handle, assoc);
        }

        public Association Get(string handle)
        {
            Association assoc = null;

            if (this.assocs.Contains(handle))
                assoc = (Association) this.assocs[handle];

            return assoc;
        }

        public bool Remove(string handle)
        {
            bool present = this.assocs.Contains(handle);

            this.assocs.Remove(handle);

            return present;
        }

        public Association Best()
        {
            Association best = null;

            foreach (Association assoc in this.assocs.Values)
            {
                if (best == null || best.Issued < assoc.Issued)
                    best = assoc;
            }

            return best;
        }

    }
   
}
