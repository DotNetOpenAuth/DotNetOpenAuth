namespace OAuthConsumer {
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

	public partial class SignInWithTwitter : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (TwitterConsumer.IsTwitterConsumerConfigured) {
				this.MultiView1.ActiveViewIndex = 1;

				if (!IsPostBack) {
					string screenName;
					int userId;
					if (TwitterConsumer.TryFinishSignInWithTwitter(out screenName, out userId)) {
						this.loggedInPanel.Visible = true;
						this.loggedInName.Text = screenName;

						// In a real app, the Twitter username would likely be used
						// to log the user into the application.
						////FormsAuthentication.RedirectFromLoginPage(screenName, false);
					}
				}
			}
		}

		protected void signInButton_Click(object sender, ImageClickEventArgs e) {
			TwitterConsumer.StartSignInWithTwitter(this.forceLoginCheckbox.Checked).Send();
		}
	}
}