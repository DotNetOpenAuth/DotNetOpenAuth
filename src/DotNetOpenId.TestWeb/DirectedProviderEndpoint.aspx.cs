using System;
using System.Web.UI;
using DotNetOpenId;
using DotNetOpenId.Provider;

public partial class DirectedProviderEndpoint : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		TestSupport.Scenarios scenario = (TestSupport.Scenarios)Enum.Parse(typeof(TestSupport.Scenarios), Request.QueryString["user"]);
		UriBuilder providerEndpoint = new UriBuilder(Request.Url);
		providerEndpoint.Query = "user=" + scenario;
		OpenIdProvider provider = new OpenIdProvider(TestSupport.ProviderStoreContext, providerEndpoint.Uri,
			Request.Url, Request.HttpMethod == "GET" ? Request.QueryString : Request.Form);
		if (provider.Request != null) {
			if (!provider.Request.IsResponseReady) {
				var idreq = provider.Request as IAuthenticationRequest;
				idreq.ClaimedIdentifier = new Uri(Request.Url, Page.ResolveUrl("~/DirectedIdentityEndpoint.aspx?user=" + scenario + "&version=" + ProtocolVersion.V20));

				switch (scenario) {
					case TestSupport.Scenarios.AutoApproval:
						// immediately approve
						idreq.IsAuthenticated = true;
						break;
					case TestSupport.Scenarios.AutoApprovalAddFragment:
						idreq.SetClaimedIdentifierFragment("frag");
						idreq.IsAuthenticated = true;
						break;
					case TestSupport.Scenarios.ApproveOnSetup:
						idreq.IsAuthenticated = !idreq.Immediate;
						break;
					case TestSupport.Scenarios.AlwaysDeny:
						idreq.IsAuthenticated = false;
						break;
					case TestSupport.Scenarios.ExtensionFullCooperation:
					case TestSupport.Scenarios.ExtensionPartialCooperation:
						throw new NotImplementedException();
						//idreq.IsAuthenticated = true;
						//break;
					default:
						throw new InvalidOperationException("Unrecognized scenario");
				}
			}
			provider.Request.Response.Send();
		}
	}
}
