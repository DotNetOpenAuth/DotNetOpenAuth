using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.RelyingParty;

public partial class OP_POSTAssertion : System.Web.UI.Page {
	protected void beginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		// Force callback URL to be just under the limit, which will cause the
		// OP to be over the limit, causing the OP to send a POST with the assertion.
		var request = openIdTextBox.CreateRequest();
		if (request != null) {
			request.AddCallbackArguments("op_endpoint", request.Provider.Uri.AbsoluteUri);
			request.AddCallbackArguments("version", request.Provider.Version.ToString());
			if (includeMultibyteCharacters.Checked) {
				request.AddCallbackArguments("utf8", "true");
				request.AddCallbackArguments("ç", "á");
			}

			int argsize = Convert.ToInt32(callbackArgumentSize.Text);
			request.AddCallbackArguments("inflate", new string('a', argsize));
		}

		openIdTextBox.LogOn();
	}

	protected void openIdTextBox_Response(object sender, DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs e) {
		e.Cancel = true; // avoid actually logging the user in with FormsAuthentication.

		MultiView1.SetActiveView(View2);

		// Check that this was a POST, and that expected callback arguments are present
		if (string.IsNullOrEmpty(e.Response.GetCallbackArgument("op_endpoint"))) {
			testResultDisplay.Pass = false;
			testResultDisplay.Details = "OP dropped the callback arguments or moved them to the POST entity.";
			return;
		}
		if (Request.HttpMethod != "POST") {
			testResultDisplay.Pass = false;
			testResultDisplay.Details = "OP sent GET request.  Try increasing the request inflation size to force a POST.";
			return;
		}

		testResultDisplay.Pass = e.Response.Status == AuthenticationStatus.Authenticated;
		testResultDisplay.ProviderEndpoint = new Uri(e.Response.GetCallbackArgument("op_endpoint"));
		testResultDisplay.ProtocolVersion = new Version(e.Response.GetCallbackArgument("version"));
		if (e.Response.Exception != null) {
			testResultDisplay.Details = e.Response.Exception.Message;
		}

		if (e.Response.Status == AuthenticationStatus.Authenticated && e.Response.GetCallbackArgument("utf8") == "true") {
			// Note that if an OP private association was used for signing and direct verification,
			// this test for proper signing of UTF-8 characters is not so meaningful, since we don't
			// get a chance to verify the signature ourselves.
			if (e.Response.GetCallbackArgument("ç") != "á") {
				testResultDisplay.Pass = false;
				testResultDisplay.Details = "Multi-byte UTF-8 characters in assertion are missing.";
			}
		}
	}
}
