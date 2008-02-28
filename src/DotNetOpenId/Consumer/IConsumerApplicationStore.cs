using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Consumer {
	public interface IConsumerApplicationStore : IAssociationStore<Uri>, INonceStore {
	}
}
