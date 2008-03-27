using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Specialized;

public partial class ProviderEndpoint : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {

	}
	protected void ProviderEndpoint1_AuthenticationChallenge(object sender, DotNetOpenId.Provider.AuthenticationChallengeEventArgs e) {
		TestSupport.Scenarios scenario = (TestSupport.Scenarios)Enum.Parse(typeof(TestSupport.Scenarios), 
			new Uri(e.Request.ClaimedIdentifier).AbsolutePath.TrimStart('/'));
		switch (scenario) {
			case TestSupport.Scenarios.AutoApproval:
				// immediately approve
				e.Request.IsAuthenticated = true;
				break;
			case TestSupport.Scenarios.ApproveOnSetup:
				e.Request.IsAuthenticated = !e.Request.Immediate;
				break;
			case TestSupport.Scenarios.AlwaysDeny:
				e.Request.IsAuthenticated = false;
				break;
			default:
				throw new InvalidOperationException("Unrecognized scenario");
		}
		e.Request.Response.Send();
	}
}
