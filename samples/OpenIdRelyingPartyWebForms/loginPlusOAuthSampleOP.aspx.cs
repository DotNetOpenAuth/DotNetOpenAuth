namespace OpenIdRelyingPartyWebForms {
	using System;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;

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
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						if (!Page.IsValid) {
							return;
						}

						await this.identifierBox.LogOnAsync(Response.ClientDisconnectedToken);
					}));
		}

		protected void identifierBox_LoggingIn(object sender, OpenIdEventArgs e) {
			var consumer = CreateConsumer();
			consumer.AttachAuthorizationRequest(e.Request, "http://tempuri.org/IDataApi/GetName");
		}

		protected void identifierBox_LoggedIn(object sender, OpenIdEventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						State.FetchResponse = e.Response.GetExtension<FetchResponse>();

						var serviceDescription = new ServiceProviderDescription {
							TokenRequestEndpoint = new Uri(e.Response.Provider.Uri, "/access_token.ashx"),
						};
						var consumer = CreateConsumer();
						consumer.ServiceProvider = serviceDescription;
						AccessTokenResponse accessToken = await consumer.ProcessUserAuthorizationAsync(e.Response);
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
					}));
		}

		protected void identifierBox_Failed(object sender, OpenIdEventArgs e) {
			this.MultiView1.SetActiveView(this.AuthenticationFailed);
		}

		private static WebConsumerOpenIdRelyingParty CreateConsumer() {
			var consumer = new WebConsumerOpenIdRelyingParty();
			consumer.ConsumerKey = new Uri(HttpContext.Current.Request.Url, HttpContext.Current.Request.ApplicationPath).AbsoluteUri;
			consumer.ConsumerSecret = "some crazy secret";
			return consumer;
		}
	}
}
