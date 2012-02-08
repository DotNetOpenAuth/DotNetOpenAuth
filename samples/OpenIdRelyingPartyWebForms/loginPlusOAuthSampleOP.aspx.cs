namespace OpenIdRelyingPartyWebForms {
	using System;
	using System.Web.Security;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public partial class loginPlusOAuthSampleOP : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
		}

		protected void beginButton_Click(object sender, EventArgs e) {
			if (!Page.IsValid) {
				return;
			}

			this.identifierBox.LogOn();
		}

		protected void identifierBox_LoggingIn(object sender, OpenIdEventArgs e) {
			ServiceProviderDescription serviceDescription = new ServiceProviderDescription {
				TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
			};

			var consumer = new WebConsumerOpenIdRelyingParty(serviceDescription, Global.OwnSampleOPHybridTokenManager);
			consumer.AttachAuthorizationRequest(e.Request, "http://tempuri.org/IDataApi/GetName");
		}

		protected void identifierBox_LoggedIn(object sender, OpenIdEventArgs e) {
			State.FetchResponse = e.Response.GetExtension<FetchResponse>();

			ServiceProviderDescription serviceDescription = new ServiceProviderDescription {
				AccessTokenEndpoint = new MessageReceivingEndpoint(new Uri(e.Response.Provider.Uri, "/access_token.ashx"), HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest),
				TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
			};
			var consumer = new WebConsumerOpenIdRelyingParty(serviceDescription, Global.OwnSampleOPHybridTokenManager);

			AuthorizedTokenResponse accessToken = consumer.ProcessUserAuthorization(e.Response);
			if (accessToken != null) {
				this.MultiView1.SetActiveView(this.AuthorizationGiven);

				// At this point, the access token would be somehow associated with the user
				// account at the RP.
				////Database.Associate(e.Response.ClaimedIdentifier, accessToken.AccessToken);
			} else {
				this.MultiView1.SetActiveView(this.AuthorizationDenied);
			}

			// Avoid the redirect
			e.Cancel = true;
		}

		protected void identifierBox_Failed(object sender, OpenIdEventArgs e) {
			this.MultiView1.SetActiveView(this.AuthenticationFailed);
		}
	}
}
