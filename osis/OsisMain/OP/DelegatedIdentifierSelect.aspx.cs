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

public partial class OP_DelegatedIdentifierSelect : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			identifierBox.Focus();

			OpenIdRelyingParty rp = new OpenIdRelyingParty();
			try {
				var message = rp.Channel.ReadFromRequest();
				if (message != null) {
					MultiView1.ActiveViewIndex = 1;
					var positive = message as PositiveAssertionResponse;
					if (positive == null || positive.ClaimedIdentifier != GetVanityUrl(positive.ProviderEndpoint)) {
						// OP either reported an error or sent a positive assertion
						// with a modified claimed_id
						testResultDisplay.Pass = true;
						testResultDisplay.ProviderEndpoint = positive.ProviderEndpoint;
						testResultDisplay.ProtocolVersion = positive.Version;
						testResultDisplay.Details = positive != null ? "claimed_id = " + positive.ClaimedIdentifier : "auth failed";
					} else {
						// A positive assertion with the original claimed_id was sent.
						testResultDisplay.Pass = false;
						testResultDisplay.Details = "Positive assertion with original claimed_id received.";
					}
				}
			} catch (ProtocolException ex) {
				// OP either reported an error or sent a positive assertion
				// with a modified claimed_id
				MultiView1.ActiveViewIndex = 1;
				testResultDisplay.Pass = true;
				testResultDisplay.Details = "OP returned an error: " + ex.Message;
			}
		}
	}

	protected void beginButton_Click(object sender, EventArgs e) {
		OpenIdRelyingParty rp = new OpenIdRelyingParty();
		Identifier opIdentifier = identifierBox.Text;
		ServiceEndpoint opEndpoint = opIdentifier.Discover(rp.Channel.WebRequestHandler).FirstOrDefault(ep => ep.ClaimedIdentifier == Protocol.V20.ClaimedIdentifierForOPIdentifier);
		if (opEndpoint == null) {
			opIdentifierRequired.Visible = true;
			return;
		}

		CheckIdRequest req = new CheckIdRequestNoCheck(opEndpoint.Version, opEndpoint.ProviderEndpoint, AuthenticationRequestMode.Setup);
		req.LocalIdentifier = Protocol.V20.ClaimedIdentifierForOPIdentifier;
		req.ReturnTo = new Uri(Request.Url, Request.Url.AbsolutePath);
		req.Realm = req.ReturnTo;

		// Force the claimed_id to be something that would simulate delegation.
		req.ClaimedIdentifier = GetVanityUrl(opEndpoint.ProviderEndpoint);

		rp.Channel.Send(req);
	}

	private Uri GetVanityUrl(Uri providerEndpoint) {
		return new Uri(
			Request.Url,
			Response.ApplyAppPathModifier(
				"~/OP/DelegatedIdSelect.aspx?ep=" + HttpUtility.UrlEncode(providerEndpoint.AbsoluteUri)));
	}
}
