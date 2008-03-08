using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	public interface IRelyingPartyApplicationStore : IAssociationStore<Uri>, INonceStore {
	}
}
