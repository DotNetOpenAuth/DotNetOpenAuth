using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId.Messages;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.Messaging;

public partial class OP_AssociateHttpNoEncryption : System.Web.UI.Page {
	OpenIdRelyingParty rp = new OpenIdRelyingParty();

	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			identifierBox.Focus();
		}
	}
	protected void beginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		IXrdsProviderEndpoint endpoint = DiscoverHttpEndpoint(identifierBox.Text);
		if (endpoint == null) {
			this.errorLabel.Text = "No HTTP provider endpoint found.";
			this.errorLabel.Visible = true;
		} else {
			Protocol protocol = Protocol.Lookup(endpoint.Version);
			testResultDisplay.ProviderEndpoint = endpoint.Uri;
			testResultDisplay.ProtocolVersion = endpoint.Version;
			var associate = new AssociateUnencryptedRequestNoCheck(endpoint.Version, endpoint.Uri) {
				AssociationType = protocol.Args.SignatureAlgorithm.HMAC_SHA1,
			};

			try {
				var response = rp.Channel.Request<DirectErrorResponse>(associate);
				testResultDisplay.Pass = true;
				testResultDisplay.Details = response.ErrorMessage;
			} catch (ProtocolException ex) {
				testResultDisplay.Pass = false;
				testResultDisplay.Details = ex.Message;
			}

			MultiView1.ActiveViewIndex = 1;
		}
	}

	private IXrdsProviderEndpoint DiscoverHttpEndpoint(Identifier identifier) {
		List<ServiceEndpoint> endpoints = identifier.Discover(rp.Channel.WebRequestHandler).ToList();
		foreach (ServiceEndpoint endpoint in identifier.Discover(rp.Channel.WebRequestHandler)) {
			if (endpoint.ProviderEndpoint.Scheme == "http") {
				return endpoint;
			}
		}
		if (endpoints.Count > 0) {
			// No HTTP endpoint.  Make one up by changing an HTTPS one to HTTP.
			// TODO: 
		}

		return null;
	}
}
