using System;
using System.Collections.Generic;
using DotNetOpenId.Extensions.AttributeExchange;
using DotNetOpenId.Extensions.SimpleRegistration;
using SregDemandLevel = DotNetOpenId.Extensions.SimpleRegistration.DemandLevel;
using DotNetOpenId.Extensions.ProviderAuthenticationPolicy;
using System.Globalization;
using DotNetOpenId;

public partial class ProviderEndpoint : System.Web.UI.Page {
	protected void ProviderEndpoint1_AuthenticationChallenge(object sender, DotNetOpenId.Provider.AuthenticationChallengeEventArgs e) {
		if (!e.Request.IsReturnUrlDiscoverable) {
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
				"return_to could not be verified using RP discovery realm {0}.", e.Request.Realm));
		}
		TestSupport.Scenarios scenario = (TestSupport.Scenarios)Enum.Parse(typeof(TestSupport.Scenarios), 
			new Uri(e.Request.LocalIdentifier).AbsolutePath.TrimStart('/'));
		switch (scenario) {
			case TestSupport.Scenarios.AutoApproval:
				e.Request.IsAuthenticated = true;
				break;
			case TestSupport.Scenarios.ApproveOnSetup:
				e.Request.IsAuthenticated = !e.Request.Immediate;
				break;
			default:
				// All other scenarios are done programmatically only.
				throw new InvalidOperationException("Unrecognized scenario");
		}
		e.Request.Response.Send();
	}
}
