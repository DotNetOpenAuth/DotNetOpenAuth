namespace OpenIdProviderWebForms {
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.Web.Security;
	using System.Web.UI;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Provider;
	using OpenIdProviderWebForms.Code;

	/// <summary>
	/// Page for giving the user the option to continue or cancel out of authentication with a consumer.
	/// </summary>
	public partial class decide : Page {
		protected void Page_Load(object src, EventArgs e) {
			this.RegisterAsyncTask(new PageAsyncTask(async ct => {
				if (ProviderEndpoint.PendingRequest == null) {
					// Response.Redirect(string) throws ThreadInterruptedException, and "async void Page_Load" doesn't properly catch it.
					this.Response.RedirectLocation = "/";
					this.Response.StatusCode = (int)HttpStatusCode.Redirect;
					this.Context.ApplicationInstance.CompleteRequest();
					return;
				}

				this.relyingPartyVerificationResultLabel.Text =
					await ProviderEndpoint.PendingRequest.IsReturnUrlDiscoverableAsync(new DefaultOpenIdHostFactories()) == RelyingPartyDiscoveryResult.Success ? "passed" : "failed";

				this.realmLabel.Text = ProviderEndpoint.PendingRequest.Realm.ToString();

				var oauthRequest = OAuthHybrid.ServiceProvider.ReadAuthorizationRequest(ProviderEndpoint.PendingRequest);
				if (oauthRequest != null) {
					this.OAuthPanel.Visible = true;
				}

				if (ProviderEndpoint.PendingAuthenticationRequest != null) {
					if (ProviderEndpoint.PendingAuthenticationRequest.IsDirectedIdentity) {
						ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier = Code.Util.BuildIdentityUrl();
					}
					this.identityUrlLabel.Text = ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier.ToString();

					// check that the logged in user is the same as the user requesting authentication to the consumer. If not, then log them out.
					if (!string.Equals(User.Identity.Name, Code.Util.ExtractUserName(ProviderEndpoint.PendingAuthenticationRequest.LocalIdentifier), StringComparison.OrdinalIgnoreCase)) {
						FormsAuthentication.SignOut();
						Response.Redirect(Request.Url.AbsoluteUri);
					}
				} else {
					this.identityUrlLabel.Text = "(not applicable)";
					this.siteRequestLabel.Text = "A site has asked for information about you.";
				}

				// if simple registration fields were used, then prompt the user for them
				var requestedFields = ProviderEndpoint.PendingRequest.GetExtension<ClaimsRequest>();
				if (requestedFields != null) {
					this.profileFields.Visible = true;
					this.profileFields.SetRequiredFieldsFromRequest(requestedFields);
					if (!IsPostBack) {
						var sregResponse = requestedFields.CreateResponse();

						// We MAY not have an entry for this user if they used Yubikey to log in.
						MembershipUser user = Membership.GetUser();
						if (user != null) {
							sregResponse.Email = Membership.GetUser().Email;
						}
						this.profileFields.SetOpenIdProfileFields(sregResponse);
					}
				}
			}));
		}

		protected void Yes_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						if (!Page.IsValid || ProviderEndpoint.PendingRequest == null) {
							return;
						}

						if (this.OAuthPanel.Visible) {
							string grantedScope = null;
							if (this.oauthPermission.Checked) {
								// This SIMPLE sample merely uses the realm as the consumerKey,
								// but in a real app this will probably involve a database lookup to translate
								// the realm to a known consumerKey.
								grantedScope = string.Empty; // we don't scope individual access rights on this sample
							}

							OAuthHybrid.ServiceProvider.AttachAuthorizationResponse(ProviderEndpoint.PendingRequest, grantedScope);
						}

						var sregRequest = ProviderEndpoint.PendingRequest.GetExtension<ClaimsRequest>();
						ClaimsResponse sregResponse = null;
						if (sregRequest != null) {
							sregResponse = this.profileFields.GetOpenIdProfileFields(sregRequest);
							ProviderEndpoint.PendingRequest.AddResponseExtension(sregResponse);
						}
						var papeRequest = ProviderEndpoint.PendingRequest.GetExtension<PolicyRequest>();
						PolicyResponse papeResponse = null;
						if (papeRequest != null) {
							papeResponse = new PolicyResponse();
							papeResponse.NistAssuranceLevel = NistAssuranceLevel.InsufficientForLevel1;
							ProviderEndpoint.PendingRequest.AddResponseExtension(papeResponse);
						}

						if (ProviderEndpoint.PendingAuthenticationRequest != null) {
							ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = true;
						} else {
							ProviderEndpoint.PendingAnonymousRequest.IsApproved = true;
						}
						Debug.Assert(
							ProviderEndpoint.PendingRequest.IsResponseReady, "Setting authentication should be all that's necessary.");

						var provider = new ProviderEndpoint();
						var response = await provider.PrepareResponseAsync();
						await response.SendAsync();
					}));
		}

		protected void No_Click(object sender, EventArgs e) {
			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct => {
						if (ProviderEndpoint.PendingRequest == null) {
							return;
						}

						if (ProviderEndpoint.PendingAuthenticationRequest != null) {
							ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = false;
						} else {
							ProviderEndpoint.PendingAnonymousRequest.IsApproved = false;
						}
						Debug.Assert(
							ProviderEndpoint.PendingRequest.IsResponseReady, "Setting authentication should be all that's necessary.");
						var provider = new ProviderEndpoint();
						var response = await provider.PrepareResponseAsync();
						await response.SendAsync();
					}));
		}
	}
}