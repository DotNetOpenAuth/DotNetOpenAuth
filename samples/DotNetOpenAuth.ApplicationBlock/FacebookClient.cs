using System.Web;
using DotNetOpenAuth.OAuthWrap;

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	public class FacebookClient : WebAppClient {
		private static readonly AuthorizationServerDescription FacebookDescription = new AuthorizationServerDescription {
			TokenEndpoint = new Uri("https://graph.facebook.com/oauth/access_token"),
			AuthorizationEndpoint = new Uri("https://graph.facebook.com/oauth/authorize"),
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="FacebookClient"/> class.
		/// </summary>
		public FacebookClient() : base(FacebookDescription) {
			this.TokenManager = new TokenManager();
		}
	}
}
