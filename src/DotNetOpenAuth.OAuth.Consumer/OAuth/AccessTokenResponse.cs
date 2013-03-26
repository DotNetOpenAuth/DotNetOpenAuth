namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class AccessTokenResponse {
		public AccessTokenResponse(string accessToken, string tokenSecret, NameValueCollection extraData) {
			this.AccessToken = new AccessToken(accessToken, tokenSecret);
			this.ExtraData = extraData;
		}

		public AccessToken AccessToken { get; set; }

		public NameValueCollection ExtraData { get; set; }
	}
}
