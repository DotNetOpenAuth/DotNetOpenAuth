using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.OpenId.Messages;

/// <summary>
/// Summary description for PositiveAssertionResponseNoCheck
/// </summary>
public class PositiveAssertionResponseNoCheck : PositiveAssertionResponse {
	public PositiveAssertionResponseNoCheck(CheckIdRequest request) : base(request) {
	}

	public override void EnsureValidMessage() {
		// We deliberately do NOT call the base method so it doesn't throw
		// when we do illegal stuff for the test.
		////base.EnsureValidMessage();
	}
}
