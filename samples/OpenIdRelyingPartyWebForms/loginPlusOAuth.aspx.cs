namespace OpenIdRelyingPartyWebForms {
	using System;
	using System.Web.Security;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public partial class loginPlusOAuth : System.Web.UI.Page {
		private const string GoogleOPIdentifier = "https://www.google.com/accounts/o8/id";
		private static readonly OpenIdRelyingParty relyingParty = new OpenIdRelyingParty();

		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack && string.Equals(Request.Url.Host, "localhost", StringComparison.OrdinalIgnoreCase)) {
				// Disable the button since the scenario won't work under localhost,
				// and this will help encourage the user to read the the text above the button.
				this.beginButton.Enabled = false;
			}

			IAuthenticationResponse authResponse = relyingParty.GetResponse();
			if (authResponse != null) {
				switch (authResponse.Status) {
					case AuthenticationStatus.Authenticated:
						State.FetchResponse = authResponse.GetExtension<FetchResponse>();
						AuthorizedTokenResponse accessToken = Global.GoogleWebConsumer.ProcessUserAuthorization(authResponse);
						if (accessToken != null) {
							State.GoogleAccessToken = accessToken.AccessToken;
							FormsAuthentication.SetAuthCookie(authResponse.ClaimedIdentifier, false);
							Response.Redirect("~/MembersOnly/DisplayGoogleContacts.aspx");
						} else {
							MultiView1.SetActiveView(AuthorizationDenied);
						}
						break;
					case AuthenticationStatus.Canceled:
					case AuthenticationStatus.Failed:
					default:
						this.MultiView1.SetActiveView(this.AuthenticationFailed);
						break;
				}
			}
		}

		protected void beginButton_Click(object sender, EventArgs e) {
			this.GetGoogleRequest().RedirectToProvider();
		}

		private IAuthenticationRequest GetGoogleRequest() {
			// Google requires that the realm and consumer key be equal,
			// so we constrain the realm to match the realm in the web.config file.
			// This does mean that the return_to URL must also fall under the key,
			// which means this sample will only work on a public web site
			// that is properly registered with Google.
			// We will customize the realm to use http or https based on what the
			// return_to URL will be (which will be this page).
			Realm realm = Request.Url.Scheme + Uri.SchemeDelimiter + Global.GoogleTokenManager.ConsumerKey + "/";
			IAuthenticationRequest authReq = relyingParty.CreateRequest(GoogleOPIdentifier, realm);

			// Prepare the OAuth extension
			string scope = GoogleConsumer.GetScopeUri(GoogleConsumer.Applications.Contacts);
			Global.GoogleWebConsumer.AttachAuthorizationRequest(authReq, scope);

			// We also want the user's email address
			var fetch = new FetchRequest();
			fetch.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
			authReq.AddExtension(fetch);

			return authReq;
		}
	}
}
