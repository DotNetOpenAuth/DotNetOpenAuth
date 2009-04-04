namespace OpenIdProviderWebForms {
	using System;
	using DotNetOpenAuth.OpenId.Provider;
	using OpenIdProviderWebForms.Code;

	/// <summary>
	/// This page is a required as part of the service discovery phase of the openid protocol (step 1).
	/// </summary>
	/// <remarks>
	/// <para>The XRDS (or Yadis) content is also rendered to provide the consumer with an alternative discovery mechanism. The Yadis protocol allows the consumer 
	/// to provide the user with a more flexible range of authentication mechanisms (which ever has been defined in xrds.aspx). See http://en.wikipedia.org/wiki/Yadis.</para>
	/// </remarks>
	public partial class user : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			this.usernameLabel.Text = Util.ExtractUserName(Page.Request.Url);
		}

		protected void IdentityEndpoint20_NormalizeUri(object sender, IdentityEndpointNormalizationEventArgs e) {
			string username = Util.ExtractUserName(Page.Request.Url);
			e.NormalizedIdentifier = new Uri(Util.BuildIdentityUrl(username));
		}
	}
}