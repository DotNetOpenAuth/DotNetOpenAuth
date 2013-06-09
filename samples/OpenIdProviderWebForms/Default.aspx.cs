namespace OpenIdProviderWebForms {
	using System;
	using System.Threading.Tasks;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;
	using OpenIdProviderWebForms.Code;

	/// <summary>
	/// Page for handling logins to this server. 
	/// </summary>
	public partial class _default : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (Request.QueryString["rp"] != null) {
				this.RegisterAsyncTask(
					new PageAsyncTask(
						async ct => {
							if (Page.User.Identity.IsAuthenticated) {
								await this.SendAssertionAsync(Request.QueryString["rp"]);
							} else {
								FormsAuthentication.RedirectToLoginPage();
							}
						}));
			} else {
				TextBox relyingPartySite = (TextBox)this.loginView.FindControl("relyingPartySite");
				if (relyingPartySite != null) {
					relyingPartySite.Focus();
				}
			}
		}

		protected async void sendAssertionButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						TextBox relyingPartySite = (TextBox)this.loginView.FindControl("relyingPartySite");
						await this.SendAssertionAsync(relyingPartySite.Text);
					}));
		}

		private async Task SendAssertionAsync(string relyingPartyRealm) {
			Uri providerEndpoint = new Uri(Request.Url, Page.ResolveUrl("~/server.aspx"));
			OpenIdProvider op = new OpenIdProvider();
			try {
				// Send user input through identifier parser so we accept more free-form input.
				string rpSite = Identifier.Parse(relyingPartyRealm);
				var response = await op.PrepareUnsolicitedAssertionAsync(providerEndpoint, rpSite, Util.BuildIdentityUrl(), Util.BuildIdentityUrl());
				await response.SendAsync();
				this.Context.Response.End();
			} catch (ProtocolException ex) {
				Label errorLabel = (Label)this.loginView.FindControl("errorLabel");
				errorLabel.Visible = true;
				errorLabel.Text = ex.Message;
			}
		}
	}
}