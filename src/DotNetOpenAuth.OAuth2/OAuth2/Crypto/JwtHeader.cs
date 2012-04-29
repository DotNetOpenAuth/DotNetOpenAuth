namespace DotNetOpenAuth.OAuth2.Crypto {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class JwtHeader : JwtMessageBase {
		internal JwtHeader() {
			this.Type = "JWT";
		}

		[MessagePart("typ")]
		internal string Type { get; set; }
	}
}
