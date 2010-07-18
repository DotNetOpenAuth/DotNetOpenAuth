namespace MvcRelyingParty.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	public class AccountAuthorizeModel {
		public string ClientApp { get; set; }

		public string Scope { get; set; }
	}
}
