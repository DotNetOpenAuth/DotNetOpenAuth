namespace OpenIdProviderWebForms {
	using System;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// This page is a required as part of the service discovery phase of the openid protocol (step 1).
	/// </summary>
	/// <remarks>
	/// <para>How does a url like http://www.myserver.com/user/bob map to  http://www.myserver.com/user.aspx?username=bob ?
	/// Check out gobal.asax and the URLRewriter class. Essentially there's a little framework that allows for URLRewrting using the HttpContext.Current.RewritePath method.</para>
	/// <para>A url such as http://www.myserver.com/user/bob which is entered on  the consumer side will cause this page to be invoked.
	/// This page must be parsed by the openid compatible consumer and the url of the openid server is extracted from href in:  rel="openid.server" href="?".
	/// It is the responsibility of the consumer to redirect the user to this url.</para>
	/// <para>The XRDS (or Yadis) content is also rendered to provide the consumer with an alternative discovery mechanism. The Yadis protocol allows the consumer 
	/// to provide the user with a more flexible range of authentication mechanisms (which ever has been defined in xrds.aspx). See http://en.wikipedia.org/wiki/Yadis.</para>
	/// </remarks>
	public partial class user : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			this.usernameLabel.Text = Request.QueryString["username"];
		}

		protected void IdentityEndpoint20_NormalizeUri(object sender, IdentityEndpointNormalizationEventArgs e) {
			// This sample Provider has a custom policy for normalizing URIs, which is that the whole
			// path of the URI be lowercase except for the first letter of the username.
			UriBuilder normalized = new UriBuilder(e.UserSuppliedIdentifier);
			string username = Request.QueryString["username"].TrimEnd('/').ToLowerInvariant();
			username = username.Substring(0, 1).ToUpperInvariant() + username.Substring(1);
			normalized.Path = "/user/" + username;
			normalized.Scheme = "http"; // for a real Provider, this should be HTTPS if supported.
			e.NormalizedIdentifier = normalized.Uri;
		}
	}
}