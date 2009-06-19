using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Messages;
using DotNetOpenAuth.OpenId.RelyingParty;

public partial class OP_DirectMessageErrors : System.Web.UI.Page {
	protected void beginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		ServiceEndpoint endpoint;
		var rp = new OpenIdRelyingParty();
		try {
			Identifier identifier = identifierBox.Text;
			endpoint = identifier.Discover(rp.Channel.WebRequestHandler).First();
		} catch (ProtocolException ex) {
			errorLabel.Text = ex.Message;
			errorLabel.Visible = true;
			return;
		}

		MultiView1.ActiveViewIndex = 1;
		testResultDisplay.ProviderEndpoint = endpoint.ProviderDescription.Endpoint;
		testResultDisplay.ProtocolVersion = endpoint.Version;

		try {
			var response = rp.Channel.Request<DirectErrorResponse>(new BadRequest(endpoint.ProviderDescription.Endpoint));
			testResultDisplay.Pass = true;
			testResultDisplay.Details = "OP returned error: " + response.ErrorMessage;
		} catch (ProtocolException ex) {
			testResultDisplay.Pass = false;
			testResultDisplay.Details = ex.Message;
		}
	}
}
