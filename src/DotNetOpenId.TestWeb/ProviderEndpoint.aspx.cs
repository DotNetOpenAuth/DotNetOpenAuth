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
using System.Collections.Generic;

public partial class ProviderEndpoint : System.Web.UI.Page {
	const string nicknameTypeUri = "http://axschema.org/namePerson/friendly";
	const string emailTypeUri = "http://axschema.org/contact/email";

	IDictionary<string, AttributeValues> storedAttributes {
		get {
			var atts = (Dictionary<string, AttributeValues>)Application["storedAttributes"];
			if (atts == null) {
				atts = new Dictionary<string, AttributeValues>();
				Application["storedAttributes"] = atts;
			}
			return atts;
		}
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
				if (sregRequest != null) {
					if (sregRequest.FullName != SimpleRegistrationRequest.NoRequest)
						sregResponse.FullName = "Andrew Arnott";
					if (sregRequest.Email != SimpleRegistrationRequest.NoRequest)
						sregResponse.Email = "andrewarnott@gmail.com";
				}
				if (aeFetchRequest != null) {
					var att = aeFetchRequest.GetAttribute(nicknameTypeUri);
					if (att != null)
						aeFetchResponse.AddAttribute(att.Respond("Andrew"));
					att = aeFetchRequest.GetAttribute(emailTypeUri);
					if (att != null) {
						string[] emails = new[] { "a@a.com", "b@b.com" };
						string[] subset = new string[Math.Min(emails.Length, att.Count)];
						Array.Copy(emails, subset, subset.Length);
						aeFetchResponse.AddAttribute(att.Respond(subset));
					}
					foreach (var att2 in aeFetchRequest.Attributes) {
						if (storedAttributes.ContainsKey(att2.TypeUri))
							aeFetchResponse.AddAttribute(storedAttributes[att2.TypeUri]);
					}
				}
				break;
			case TestSupport.Scenarios.ExtensionPartialCooperation:
				if (sregRequest != null) {
					if (sregRequest.FullName == SimpleRegistrationRequest.Require)
						sregResponse.FullName = "Andrew Arnott";
					if (sregRequest.Email == SimpleRegistrationRequest.Require)
						sregResponse.Email = "andrewarnott@gmail.com";
				}
				if (aeFetchRequest != null) {
					var att = aeFetchRequest.GetAttribute(nicknameTypeUri);
					if (att != null && att.IsRequired)
						aeFetchResponse.AddAttribute(att.Respond("Andrew"));
					att = aeFetchRequest.GetAttribute(emailTypeUri);
					if (att != null && att.IsRequired) {
						string[] emails = new[] { "a@a.com", "b@b.com" };
						string[] subset = new string[Math.Min(emails.Length, att.Count)];
						Array.Copy(emails, subset, subset.Length);
						aeFetchResponse.AddAttribute(att.Respond(subset));
					}
					foreach (var att2 in aeFetchRequest.Attributes) {
						if (att2.IsRequired && storedAttributes.ContainsKey(att2.TypeUri))
							aeFetchResponse.AddAttribute(storedAttributes[att2.TypeUri]);
					}
				}
				break;
		}
		if (aeStoreRequest != null) {
			foreach (var att in aeStoreRequest.Attributes) {
				storedAttributes[att.TypeUri] = att;
			}
			aeStoreResponse.Succeeded = true;
		} 
		if (sregRequest != null) sregResponse.AddToResponse(request);
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
