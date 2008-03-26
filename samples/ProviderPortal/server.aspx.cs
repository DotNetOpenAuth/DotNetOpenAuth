using DotNetOpenId.Provider;

/// <summary>
/// This is the primary page for this open-id provider.
/// This page is responsible for handling all open-id compliant requests.
/// </summary>
public partial class server : System.Web.UI.Page {
	protected void Page_Load(object src, System.EventArgs evt) {
		serverEndpointUrl.Text = Request.Url.ToString();
	}
	protected void provider_AuthenticationChallenge(object sender, AuthenticationChallengeEventArgs e) {
		var idrequest = e.Request;
		if (idrequest.Immediate) {
			string userOwningOpenIdUrl = Util.ExtractUserName(idrequest.ClaimedIdentifier);
			// NOTE: in a production provider site, you may want to only 
			// respond affirmatively if the user has already authorized this consumer
			// to know the answer.
			idrequest.IsAuthenticated = userOwningOpenIdUrl == User.Identity.Name;
		} else {
			Response.Redirect("~/decide.aspx", true); // This ends processing on this page.
		}
	}
}