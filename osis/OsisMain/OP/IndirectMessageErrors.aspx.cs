using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Messages;
using DotNetOpenAuth.OpenId.RelyingParty;

public partial class OP_IndirectMessageErrors : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		var rp = new OpenIdRelyingParty();
		try {
			if (Request.QueryString["op_endpoint"] != null) {
				testResultDisplay.ProviderEndpoint = new Uri(Request.QueryString["op_endpoint"]);
				testResultDisplay.ProtocolVersion = new Version(Request.QueryString["version"]);
			}
			IDirectedProtocolMessage response = rp.Channel.ReadFromRequest();
			if (response != null) {
				MultiView1.ActiveViewIndex = 1;
				testResultDisplay.Pass = false;
				testResultDisplay.Details = "OP returned a non-error message: " + response.GetType().Name;
			}
		} catch (ProtocolException ex) {
			MultiView1.ActiveViewIndex = 1;
			IndirectErrorResponse errorResponse = (IndirectErrorResponse)ex.FaultedMessage;
			testResultDisplay.Pass = true;
			testResultDisplay.Details = "OP returned error: " + errorResponse.ErrorMessage;
		}
	}

	protected void beginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		var rp = new OpenIdRelyingParty();
		try {
			Identifier identifier = identifierBox.Text;
			var endpoint = identifier.Discover(rp.Channel.WebRequestHandler).First();
			UriBuilder returnTo = new UriBuilder(new Uri(Request.Url, Request.Url.AbsolutePath));
			returnTo.Query = string.Format(
				CultureInfo.InvariantCulture,
				"op_endpoint={0}&version={1}",
				Uri.EscapeDataString(endpoint.ProviderEndpoint.AbsoluteUri),
				Uri.EscapeDataString(endpoint.Version.ToString()));
			IDirectedProtocolMessage badMessage = new BadRequest(
				endpoint.ProviderEndpoint,
				returnTo.Uri);
			rp.Channel.Send(badMessage);
		} catch (ProtocolException ex) {
			errorLabel.Text = ex.Message;
			errorLabel.Visible = true;
			return;
		}
	}
}
