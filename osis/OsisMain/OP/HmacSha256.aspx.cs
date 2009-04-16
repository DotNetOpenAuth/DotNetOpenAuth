using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.Messaging;

public partial class OP_HmacSha256 : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
	}

	protected void beginButton_Click(object sender, EventArgs e) {
		if(!Page.IsValid) {
			return;
		}
		
		var rp = new OpenIdRelyingParty();
		rp.SecuritySettings.MinimumHashBitLength = 256;
		rp.SecuritySettings.MaximumHashBitLength = 256;
		Identifier identifier = identifierBox.Text;
		try {
			ServiceEndpoint endpoint = identifier.Discover(rp.Channel.WebRequestHandler).FirstOrDefault(op => op.Version.Major >= 2);
			if (endpoint == null) {
				errorLabel.Text = "No OpenID 2.0 Provider endpoint could be found with this identifier.";
				errorLabel.Visible = true;
				return;
			}
			Association association = rp.AssociationManager.CreateNewAssociation(endpoint.ProviderDescription);
			MultiView1.ActiveViewIndex = 1;
			testResultDisplay.ProviderEndpoint = endpoint.ProviderEndpoint;
			testResultDisplay.ProtocolVersion = endpoint.Version;
			if (association != null && association.HashBitLength == 256) {
				testResultDisplay.Pass = true;
			} else {
				testResultDisplay.Pass = false;
			}
		} catch (ProtocolException ex) {
			errorLabel.Text = ex.Message;
			errorLabel.Visible = true;
		}
	}
}
