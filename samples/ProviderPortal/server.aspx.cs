using DotNetOpenId.Provider;
using System;

/// <summary>
/// This is the primary page for this open-id provider.
/// This page is responsible for handling all open-id compliant requests.
/// </summary>
public partial class server : System.Web.UI.Page {
	protected void Page_Load(object src, System.EventArgs evt) {
		serverEndpointUrl.Text = Request.Url.ToString();
	}
	protected void provider_AuthenticationChallenge(object sender, AuthenticationChallengeEventArgs e) {
		Util.ProcessAuthenticationChallenge(e.Request);
	}
}