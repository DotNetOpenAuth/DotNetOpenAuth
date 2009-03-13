using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;

public partial class OP_ResponseNonce : System.Web.UI.Page {
	OpenIdRelyingParty rp = new OpenIdRelyingParty();

	protected void Page_Load(object sender, EventArgs e) {
		// Force the response_nonce to be a requirement.
		rp.SecuritySettings.MinimumRequiredOpenIdVersion = ProtocolVersion.V20;

		if (!IsPostBack) {
			identifierBox.Focus();

			IAuthenticationResponse response = rp.GetResponse();
			if (response != null) {
				var resp = (PositiveAuthenticationResponse)response;
				MultiView1.ActiveViewIndex = 1;
				testResultDisplay.Pass = response.Status == AuthenticationStatus.Authenticated;
				testResultDisplay.Details = "response_nonce: " + resp.Response.ResponseNonce;
				testResultDisplay.LoadResponse(response);
			}
		}
	}

	protected void beginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		try {
			IAuthenticationRequest req = rp.CreateRequest(identifierBox.Text);
			testResultDisplay.PrepareRequest(req);
			req.RedirectToProvider();
		} catch (ProtocolException ex) {
			errorLabel.Text = ex.Message;
			errorLabel.Visible = true;
		}
	}
}
