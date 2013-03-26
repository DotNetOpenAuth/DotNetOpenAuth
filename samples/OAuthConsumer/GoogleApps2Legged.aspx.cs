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
		protected void Page_Load(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						var google = new GoogleConsumer();
						var accessToken = await google.RequestNewClientAccountAsync();
						using (var httpClient = google.CreateHttpClient(accessToken.AccessToken)) {
							await httpClient.GetAsync("http://someUri", Response.ClientDisconnectedToken);
						}
					}));
		}

		protected void getAddressBookButton_Click(object sender, EventArgs e) {
			throw new NotImplementedException();
		}
	}
}