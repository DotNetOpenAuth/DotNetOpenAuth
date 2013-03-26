namespace OpenIdRelyingPartyWebForms {
	using System;
	using System.Threading;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public partial class NoIdentityOpenId : System.Web.UI.Page {
		protected async void Page_Load(object sender, EventArgs e) {
			this.openIdBox.Focus();
			using (OpenIdRelyingParty rp = new OpenIdRelyingParty()) {
				IAuthenticationResponse response = await rp.GetResponseAsync(new HttpRequestWrapper(this.Request), this.Response.ClientDisconnectedToken);
				if (response != null) {
					switch (response.Status) {
						case AuthenticationStatus.ExtensionsOnly:
							this.ExtensionResponsesPanel.Visible = true;

							// This is the "success" status we get when no authentication was requested.
							var sreg = response.GetExtension<ClaimsResponse>();
							if (sreg != null) {
								this.emailLabel.Text = sreg.Email;
								this.timeZoneLabel.Text = sreg.TimeZone;
								this.postalCodeLabel.Text = sreg.PostalCode;
								this.countryLabel.Text = sreg.Country;
								if (sreg.Gender.HasValue) {
									this.genderLabel.Text = sreg.Gender.Value.ToString();
								}
							}
							break;
						case AuthenticationStatus.Canceled:
							this.resultMessage.Text = "Canceled at OP.  This may be a sign that the OP doesn't support this message.";
							break;
						case AuthenticationStatus.Failed:
							this.resultMessage.Text = "OP returned a failure: " + response.Exception;
							break;
						case AuthenticationStatus.SetupRequired:
						case AuthenticationStatus.Authenticated:
						default:
							this.resultMessage.Text = "OP returned an unexpected response.";
							break;
					}
				}
			}
		}

		protected void beginButton_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						if (!this.Page.IsValid) {
							return; // don't login if custom validation failed.
						}
						try {
							using (OpenIdRelyingParty rp = new OpenIdRelyingParty()) {
								var request =
									await
									rp.CreateRequestAsync(this.openIdBox.Text, new HttpRequestWrapper(Request), Response.ClientDisconnectedToken);
								request.IsExtensionOnly = true;

								// This is where you would add any OpenID extensions you wanted
								// to include in the request.
								request.AddExtension(
									new ClaimsRequest {
										Email = DemandLevel.Request,
										Country = DemandLevel.Request,
										Gender = DemandLevel.Require,
										PostalCode = DemandLevel.Require,
										TimeZone = DemandLevel.Require,
									});

								await request.RedirectToProviderAsync(new HttpContextWrapper(Context), Response.ClientDisconnectedToken);
							}
						} catch (ProtocolException ex) {
							// The user probably entered an Identifier that 
							// was not a valid OpenID endpoint.
							this.openidValidator.Text = ex.Message;
							this.openidValidator.IsValid = false;
						}
					}));
		}

		protected void openidValidator_ServerValidate(object source, ServerValidateEventArgs args) {
			// This catches common typos that result in an invalid OpenID Identifier.
			args.IsValid = Identifier.IsValid(args.Value);
		}
	}
}
