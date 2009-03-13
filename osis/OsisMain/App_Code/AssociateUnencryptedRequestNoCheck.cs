using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.OpenId.Messages;

public class AssociateUnencryptedRequestNoCheck : AssociateUnencryptedRequest {
	public AssociateUnencryptedRequestNoCheck(Version version, Uri providerEndpoint)
		: base(version, providerEndpoint) {
	}

	public override void EnsureValidMessage() {
		// We deliberately do NOT call the base method so it doesn't throw
		// when we do illegal stuff for the test.
		////base.EnsureValidMessage();
	}
}
