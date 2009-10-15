using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace WebFormsRelyingParty {
	public partial class LoginFrame : System.Web.UI.Page, IPostBackEventHandler {
		private const string postLoginAssertionMethodName = "postLoginAssertion";

		private static readonly ProviderInfo[] providers = new ProviderInfo[] {
			new ProviderInfo("https://me.yahoo.com/", "images/yahoo.gif"),
			new ProviderInfo("https://www.google.com/accounts/o8/id", "images/google.gif"),
			new ProviderInfo(null, "images/openid.gif"),
		};

		protected void Page_Load(object sender, EventArgs e) {
			const string positiveAssertionParameterName = "positiveAssertion";
			const string originPageParameterName = "originPage";
			string script = string.Format(
				CultureInfo.InvariantCulture,
@"window.{2} = function({0}, {3}) {{
	$('#OpenIDForm')[0].target = '_top';
	$('#openidPositiveAssertion')[0].setAttribute('value', {0});
	$('#originPage')[0].setAttribute('value', {3});
	{1};
}};",
				positiveAssertionParameterName,
				this.ClientScript.GetPostBackEventReference(this, null, false),
				postLoginAssertionMethodName,
				originPageParameterName);
			this.ClientScript.RegisterClientScriptBlock(this.GetType(), "Postback", script, true);

			providerRepeater.DataSource = providers;
			providerRepeater.DataBind();

			if (!this.IsPostBack) {
				// Perform early discovery on the OPs

			}
		}

		#region IPostBackEventHandler Members

		public void RaisePostBackEvent(string eventArgument) {
			if (eventArgument == postLoginAssertionMethodName) {
				Uri assertion = new Uri(openidPositiveAssertion.Value);
				HttpRequestInfo requestInfo = new HttpRequestInfo("GET", Request.Url, assertion.PathAndQuery, new System.Net.WebHeaderCollection(), null);
				IAuthenticationResponse response = openid_identifier.RelyingParty.GetResponse(requestInfo);
				if (response.Status == AuthenticationStatus.Authenticated) {
					FormsAuthentication.SetAuthCookie(response.ClaimedIdentifier, false);
					Response.Redirect(originPage.Value);
				}
			}
		}

		#endregion

		private class ProviderInfo {
			internal ProviderInfo(string identifier, string image) {
				this.OPIdentifier = identifier;
				this.Image = image;
			}

			public string OPIdentifier { get; set; }
			public string Image { get; set; }
		}
	}
}
