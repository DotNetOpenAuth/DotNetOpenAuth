using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId;

namespace DotNetOpenId.Store
{
    public interface IAssociationStore
    {

        byte[] AuthKey { get; }
        bool IsDumb { get; }

        void StoreAssociation(Uri serverUri, Association assoc);
        Association GetAssociation(Uri serverUri);
        Association GetAssociation(Uri serverUri, string handle);
        bool RemoveAssociation(Uri serverUri, string handle);
        void StoreNonce(string nonce);
        bool UseNonce(string nonce);

    }
}
