namespace OAuthClient {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Xml.Linq;
	using System.Xml.XPath;
	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.OAuth;

	using DotNetOpenAuth.Messaging;

	public partial class SignInWithTwitter : System.Web.UI.Page {
		protected async void Page_Load(object sender, EventArgs e) {
			if (TwitterConsumer.IsTwitterConsumerConfigured) {
				this.MultiView1.ActiveViewIndex = 1;

				if (!IsPostBack) {
					var tuple = await TwitterConsumer.TryFinishSignInWithTwitterAsync(Response.ClientDisconnectedToken);
					if (tuple != null) {
						string screenName = tuple.Item1;
						int userId = tuple.Item2;
						this.loggedInPanel.Visible = true;
						this.loggedInName.Text = screenName;

						// In a real app, the Twitter username would likely be used
						// to log the user into the application.
						////FormsAuthentication.RedirectFromLoginPage(screenName, false);
					}
				}
			}
		}

		protected async void signInButton_Click(object sender, ImageClickEventArgs e) {
			var request = await TwitterConsumer.StartSignInWithTwitterAsync(this.forceLoginCheckbox.Checked, Response.ClientDisconnectedToken);
			await request.SendAsync(new HttpResponseWrapper(Response), Response.ClientDisconnectedToken);
		}
	}
}