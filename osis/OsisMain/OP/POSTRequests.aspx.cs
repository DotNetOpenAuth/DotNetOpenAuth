using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.RelyingParty;

public partial class OP_POSTRequests : System.Web.UI.Page {
	protected void beginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		// Force all requests to be POSTS
		openIdTextBox.RelyingParty.Channel.IndirectMessageGetToPostThreshold = 1;
		openIdTextBox.LogOn();
	}

	protected void openIdTextBox_LoggingIn(object sender, OpenIdEventArgs e) {
		e.Request.SetUntrustedCallbackArgument("op_endpoint", e.Request.Provider.Uri.AbsoluteUri);
		e.Request.SetUntrustedCallbackArgument("version", e.Request.Provider.Version.ToString());
	}

	protected void openIdTextBox_Response(object sender, OpenIdEventArgs e) {
		e.Cancel = true; // avoid actually logging the user in with FormsAuthentication.

		MultiView1.SetActiveView(View2);
		testResultDisplay.Pass = e.Response.Status == AuthenticationStatus.Authenticated;
		testResultDisplay.ProviderEndpoint = new Uri(e.Response.GetUntrustedCallbackArgument("op_endpoint"));
		testResultDisplay.ProtocolVersion = new Version(e.Response.GetUntrustedCallbackArgument("version"));
		if (e.Response.Exception != null) {
			testResultDisplay.Details = e.Response.Exception.Message;
		}
	}
}
