namespace OpenIdWebRingSsoRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public partial class Login : System.Web.UI.Page {
		private const string RolesAttribute = "http://samples.dotnetopenauth.net/sso/roles";

		private static OpenIdRelyingParty relyingParty = new OpenIdRelyingParty();

		static Login() {
			// Configure the RP to only allow assertions from our trusted OP endpoint.
			relyingParty.EndpointFilter = ep => ep.Uri.AbsoluteUri == ConfigurationManager.AppSettings["SsoProviderOPEndpoint"];
		}

		protected void Page_Load(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						UriBuilder returnToBuilder = new UriBuilder(Request.Url);
						returnToBuilder.Path = "/login.aspx";
						returnToBuilder.Query = null;
						returnToBuilder.Fragment = null;
						Uri returnTo = returnToBuilder.Uri;
						returnToBuilder.Path = "/";
						Realm realm = returnToBuilder.Uri;

						var response =
							await relyingParty.GetResponseAsync(new HttpRequestWrapper(Request), Response.ClientDisconnectedToken);
						if (response == null) {
							if (Request.QueryString["ReturnUrl"] != null && User.Identity.IsAuthenticated) {
								// The user must have been directed here because he has insufficient
								// permissions to access something.
								this.MultiView1.ActiveViewIndex = 1;
							} else {
								// Because this is a sample of a controlled SSO environment,
								// we don't ask the user which Provider to use... we just send
								// them straight off to the one Provider we trust.
								var request =
									await
									relyingParty.CreateRequestAsync(
										ConfigurationManager.AppSettings["SsoProviderOPIdentifier"], realm, returnTo, Response.ClientDisconnectedToken);
								var fetchRequest = new FetchRequest();
								fetchRequest.Attributes.AddOptional(RolesAttribute);
								request.AddExtension(fetchRequest);
								await request.RedirectToProviderAsync(new HttpContextWrapper(Context), Response.ClientDisconnectedToken);
							}
						} else {
							switch (response.Status) {
								case AuthenticationStatus.Canceled:
									this.errorLabel.Text = "Login canceled.";
									break;
								case AuthenticationStatus.Failed:
									this.errorLabel.Text = HttpUtility.HtmlEncode(response.Exception.Message);
									break;
								case AuthenticationStatus.Authenticated:
									IList<string> roles = null;
									var fetchResponse = response.GetExtension<FetchResponse>();
									if (fetchResponse != null) {
										if (fetchResponse.Attributes.Contains(RolesAttribute)) {
											roles = fetchResponse.Attributes[RolesAttribute].Values;
										}
									}
									if (roles == null) {
										roles = new List<string>(0);
									}

									// Apply the roles to this auth ticket
									const int TimeoutInMinutes = 100; // TODO: look up the right value from the web.config file
									var ticket = new FormsAuthenticationTicket(
										2,
										response.ClaimedIdentifier,
										DateTime.Now,
										DateTime.Now.AddMinutes(TimeoutInMinutes),
										false, // non-persistent, since login is automatic and we wanted updated roles
										string.Join(";", roles.ToArray()));

									HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
									Response.SetCookie(cookie);
									Response.Redirect(Request.QueryString["ReturnUrl"] ?? FormsAuthentication.DefaultUrl);
									break;
								default:
									break;
							}
						}
					}));
		}

		protected void retryButton_Click(object sender, EventArgs e) {
			Response.Redirect("/login.aspx");
		}
	}
}
