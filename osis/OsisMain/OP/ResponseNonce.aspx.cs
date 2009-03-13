using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.Messaging;

public partial class OP_ResponseNonce : System.Web.UI.Page {
	OpenIdRelyingParty rp = new OpenIdRelyingParty();

	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			identifierBox.Focus();

			IAuthenticationResponse response = rp.GetResponse();
			if (response != null) {
				var resp = (PositiveAuthenticationResponse)response;
				MultiView1.ActiveViewIndex = 1;
				testResultDisplay.Pass = response.Status == AuthenticationStatus.Authenticated;
				testResultDisplay.Details = "response_nonce: " + resp.Response.ResponseNonce;
				testResultDisplay.ProviderEndpoint = new Uri(resp.GetCallbackArgument("opEndpoint"));
				testResultDisplay.ProtocolVersion = new Version(resp.GetCallbackArgument("opVersion"));
			}
		}
	}

	protected void beginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		try {
			IAuthenticationRequest req = rp.CreateRequest(identifierBox.Text);
			req.AddCallbackArguments("opEndpoint", req.Provider.Uri.AbsoluteUri);
			req.AddCallbackArguments("opVersion", req.Provider.Version.ToString());
			req.RedirectToProvider();
		} catch (ProtocolException ex) {
			errorLabel.Text = ex.Message;
			errorLabel.Visible = true;
		}
	}
}
