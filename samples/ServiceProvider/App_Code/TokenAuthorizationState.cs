using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Various states an OAuth token can be in.
/// </summary>
public enum TokenAuthorizationState : int {
	UnauthorizedRequestToken = 0,
	AuthorizedRequestToken = 1,
	AccessToken = 2,
}
