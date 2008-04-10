using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// The contract for implementing a custom store for a relying party web site.
	/// </summary>
	public interface IRelyingPartyApplicationStore : IAssociationStore<Uri>, INonceStore {
	}
}
