namespace WebFormsRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;

	public partial class IssuedToken : IServiceProviderRequestToken, IServiceProviderAccessToken {
		public Uri Callback {
			get { return this.CallbackAsString != null ? new Uri(this.CallbackAsString) : null; }
			set { this.CallbackAsString = value != null ? value.AbsoluteUri : null; }
		}

		string[] IServiceProviderAccessToken.Roles {
			get {
				List<string> roles = new List<string>();

				// Include the roles the user who authorized this OAuth token has.
				// TODO: code here

				// Always add an extra role to indicate this is an OAuth-authorized request.
				// This allows us to deny access to account management pages to OAuth requests.
				roles.Add("OAuthToken");

				return roles.ToArray();
			}
		}

		string IServiceProviderAccessToken.Username {
			get {
				// We don't really have the concept of a single username, but we
				// can use any of the authentication tokens instead since that
				// is what the rest of the web site expects.
				return this.User.AuthenticationTokens.First().ClaimedIdentifier;
			}
		}

		Version IServiceProviderRequestToken.ConsumerVersion {
			get { return this.ConsumerVersionAsString != null ? new Version(this.ConsumerVersionAsString) : null; }
			set { this.ConsumerVersionAsString = value != null ? value.ToString() : null; }
		}

		string IServiceProviderRequestToken.ConsumerKey {
			get { return this.Consumer.ConsumerKey; }
		}
	}
}
