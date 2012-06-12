namespace OpenIdRelyingPartyWebForms {
	using System;
	using System.Collections.Generic;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public partial class login : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			this.OpenIdLogin1.Focus();
		}

		protected void requireSslCheckBox_CheckedChanged(object sender, EventArgs e) {
			this.OpenIdLogin1.RequireSsl = this.requireSslCheckBox.Checked;
		}

		protected void OpenIdLogin1_LoggingIn(object sender, OpenIdEventArgs e) {
			this.prepareRequest(e.Request);
		}

		/// <summary>
		/// Fired upon login.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs"/> instance containing the event data.</param>
		/// <remarks>
		/// Note, that straight after login, forms auth will redirect the user
		/// to their original page. So this page may never be rendererd.
		/// </remarks>
		protected void OpenIdLogin1_LoggedIn(object sender, OpenIdEventArgs e) {
			State.FriendlyLoginName = e.Response.FriendlyIdentifierForDisplay;
			State.ProfileFields = e.Response.GetExtension<ClaimsResponse>();
			State.PapePolicies = e.Response.GetExtension<PolicyResponse>();
		}

		private void prepareRequest(IAuthenticationRequest request) {
			// Collect the PAPE policies requested by the user.
			List<string> policies = new List<string>();
			foreach (ListItem item in this.papePolicies.Items) {
				if (item.Selected) {
					policies.Add(item.Value);
				}
			}

			// Add the PAPE extension if any policy was requested.
			var pape = new PolicyRequest();
			if (policies.Count > 0) {
				foreach (string policy in policies) {
					pape.PreferredPolicies.Add(policy);
				}
			}

			if (this.maxAuthTimeBox.Text.Length > 0) {
				pape.MaximumAuthenticationAge = TimeSpan.FromSeconds(double.Parse(this.maxAuthTimeBox.Text));
			}

			if (pape.PreferredPolicies.Count > 0 || pape.MaximumAuthenticationAge.HasValue) {
				request.AddExtension(pape);
			}
		}
	}
}