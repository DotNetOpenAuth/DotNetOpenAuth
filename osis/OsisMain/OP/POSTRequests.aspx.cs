using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.RelyingParty;

public partial class OP_POSTRequests : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {

	}

	protected void beginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		// Force all requests to be POSTS
		openIdTextBox.RelyingParty.Channel.IndirectMessageGetToPostThreshold = 1;
		var request = openIdTextBox.CreateRequest();
		if (request != null) {
			request.AddCallbackArguments("op_endpoint", request.Provider.Uri.AbsoluteUri);
			request.AddCallbackArguments("version", request.Provider.Version.ToString());
		}
		openIdTextBox.LogOn();
	}

	protected void openIdTextBox_Response(object sender, DotNetOpenAuth.OpenId.RelyingParty.OpenIdEventArgs e) {
		e.Cancel = true; // avoid actually logging the user in with FormsAuthentication.

		MultiView1.SetActiveView(View2);
		testResultDisplay.Pass = e.Response.Status == AuthenticationStatus.Authenticated;
		testResultDisplay.ProviderEndpoint = new Uri(e.Response.GetCallbackArgument("op_endpoint"));
		testResultDisplay.ProtocolVersion = new Version(e.Response.GetCallbackArgument("version"));
		if (e.Response.Exception != null) {
			testResultDisplay.Details = e.Response.Exception.Message;
		}
	}
}
