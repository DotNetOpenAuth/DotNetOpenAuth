namespace OAuthConsumer {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.Messages;

	public partial class GoogleApps2Legged : System.Web.UI.Page {
		private InMemoryTokenManager TokenManager {
			get {
				var tokenManager = (InMemoryTokenManager)Application["GoogleTokenManager"];
				if (tokenManager == null) {
					string consumerKey = ConfigurationManager.AppSettings["googleConsumerKey"];
					string consumerSecret = ConfigurationManager.AppSettings["googleConsumerSecret"];
					if (!string.IsNullOrEmpty(consumerKey)) {
						tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
						Application["GoogleTokenManager"] = tokenManager;
					}
				}

				return tokenManager;
			}
		}

		protected async void Page_Load(object sender, EventArgs e) {
			var google = new WebConsumer(GoogleConsumer.ServiceDescription, this.TokenManager);
			string accessToken = await google.RequestNewClientAccountAsync(cancellationToken: Response.ClientDisconnectedToken);
			////string tokenSecret = google.TokenManager.GetTokenSecret(accessToken);
			MessageReceivingEndpoint ep = null; // set up your authorized call here.
			var request = await google.PrepareAuthorizedRequestAsync(ep, accessToken, Response.ClientDisconnectedToken);
			using (var httpClient = google.Channel.HostFactories.CreateHttpClient()) {
				await httpClient.SendAsync(request, Response.ClientDisconnectedToken);
			}
		}
	}
}