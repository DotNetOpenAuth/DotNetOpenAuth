using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.OpenId.Messages;
using DotNetOpenAuth.OpenId.RelyingParty;

public class CheckIdRequestNoCheck : CheckIdRequest {
	public CheckIdRequestNoCheck(Version version, Uri providerEndpoint, AuthenticationRequestMode mode)  : base(version, providerEndpoint,mode) {
	}

	public override void EnsureValidMessage() {
		// We deliberately do NOT call the base method so it doesn't throw
		// when we do illegal stuff for the test.
		////base.EnsureValidMessage();
	}
}
