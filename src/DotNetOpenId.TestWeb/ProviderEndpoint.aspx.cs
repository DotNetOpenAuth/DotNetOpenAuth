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
using DotNetOpenId.Extensions;

public partial class ProviderEndpoint : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {

	}
	void respondToExtensions(DotNetOpenId.Provider.IRequest request, TestSupport.Scenarios scenario) {
		var sregRequest = SimpleRegistrationRequestFields.ReadFromRequest(request);
		var sregResponse = new SimpleRegistrationFieldValues();
		var aeFetchRequest = AttributeExchangeFetchRequest.ReadFromRequest(request);
		var aeFetchResponse = new AttributeExchangeFetchResponse();
		var aeStoreRequest = AttributeExchangeStoreRequest.ReadFromRequest(request);
		var aeStoreResponse = new AttributeExchangeStoreResponse();
		switch (scenario) {
			case TestSupport.Scenarios.ExtensionFullCooperation:
				if (sregRequest.FullName != SimpleRegistrationRequest.NoRequest)
					sregResponse.FullName = "Andrew Arnott";
				if (sregRequest.Email != SimpleRegistrationRequest.NoRequest)
					sregResponse.Email = "andrewarnott@gmail.com";
				break;
			case TestSupport.Scenarios.ExtensionPartialCooperation:
				if (sregRequest.FullName == SimpleRegistrationRequest.Require)
					sregResponse.FullName = "Andrew Arnott";
				if (sregRequest.Email == SimpleRegistrationRequest.Require)
					sregResponse.Email = "andrewarnott@gmail.com";
				break;
		}
		sregResponse.AddToResponse(request);
		if (aeFetchRequest != null) aeFetchResponse.AddToResponse(request);
		if (aeStoreRequest != null) aeStoreResponse.AddToResponse(request);
	}

	protected void ProviderEndpoint1_AuthenticationChallenge(object sender, DotNetOpenId.Provider.AuthenticationChallengeEventArgs e) {
		TestSupport.Scenarios scenario = (TestSupport.Scenarios)Enum.Parse(typeof(TestSupport.Scenarios),
			new Uri(e.Request.LocalIdentifier.ToString()).AbsolutePath.TrimStart('/'));
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
			case TestSupport.Scenarios.ExtensionFullCooperation:
			case TestSupport.Scenarios.ExtensionPartialCooperation:
				respondToExtensions(e.Request, scenario);
				e.Request.IsAuthenticated = true;
				break;
			default:
				throw new InvalidOperationException("Unrecognized scenario");
		}
		e.Request.Response.Send();
	}
}
