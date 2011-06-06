namespace OAuthAuthorizationServer.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	using DotNetOpenAuth.OAuth2.Messages;

	public class AccountAuthorizeModel {
		public string ClientApp { get; set; }

		public HashSet<string> Scope { get; set; }

		public EndUserAuthorizationRequest AuthorizationRequest { get; set; }
	}
}
