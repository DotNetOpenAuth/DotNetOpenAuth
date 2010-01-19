namespace MvcRelyingParty.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	public class AccountAuthorizeModel {
		public string ConsumerApp { get; set; }

		public bool IsUnsafeRequest { get; set; }

		public string VerificationCode { get; set; }
	}
}
