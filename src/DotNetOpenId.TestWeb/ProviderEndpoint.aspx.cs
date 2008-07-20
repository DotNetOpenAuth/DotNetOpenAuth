using System;
using System.Collections.Generic;
using DotNetOpenId.Extensions.AttributeExchange;
using DotNetOpenId.Extensions.SimpleRegistration;
using SregDemandLevel = DotNetOpenId.Extensions.SimpleRegistration.DemandLevel;
using DotNetOpenId.Extensions.ProviderAuthenticationPolicy;
using System.Globalization;

public partial class ProviderEndpoint : System.Web.UI.Page {
	const string nicknameTypeUri = WellKnownAttributes.Name.Alias;
	const string emailTypeUri = WellKnownAttributes.Contact.Email;

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
		var sregRequest = request.GetExtension<ClaimsRequest>();
		var sregResponse = sregRequest != null ? sregRequest.CreateResponse() : null;
		var aeFetchRequest = request.GetExtension<FetchRequest>();
		var aeFetchResponse = new FetchResponse();
		var aeStoreRequest = request.GetExtension<StoreRequest>();
		var aeStoreResponse = new StoreResponse();
		var papeRequest = request.GetExtension<PolicyRequest>();
		var papeResponse = new PolicyResponse();
		switch (scenario) {
			case TestSupport.Scenarios.ExtensionFullCooperation:
				if (sregRequest != null) {
					if (sregRequest.FullName != SregDemandLevel.NoRequest)
						sregResponse.FullName = "Andrew Arnott";
					if (sregRequest.Email != SregDemandLevel.NoRequest)
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
				if (papeRequest != null) {
					if (papeRequest.MaximumAuthenticationAge.HasValue) {
						papeResponse.AuthenticationTimeUtc = DateTime.UtcNow - (papeRequest.MaximumAuthenticationAge.Value - TimeSpan.FromSeconds(30));
					}
				}
				break;
			case TestSupport.Scenarios.ExtensionPartialCooperation:
				if (sregRequest != null) {
					if (sregRequest.FullName == SregDemandLevel.Require)
						sregResponse.FullName = "Andrew Arnott";
					if (sregRequest.Email == SregDemandLevel.Require)
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
		if (sregRequest != null) request.AddResponseExtension(sregResponse);
		if (aeFetchRequest != null) request.AddResponseExtension(aeFetchResponse);
		if (aeStoreRequest != null) request.AddResponseExtension(aeStoreResponse);
		if (papeRequest != null) request.AddResponseExtension(papeResponse);
	}

	protected void ProviderEndpoint1_AuthenticationChallenge(object sender, DotNetOpenId.Provider.AuthenticationChallengeEventArgs e) {
		TestSupport.Scenarios scenario = (TestSupport.Scenarios)Enum.Parse(typeof(TestSupport.Scenarios),
			new Uri(e.Request.LocalIdentifier.ToString()).AbsolutePath.TrimStart('/'));
		if (!e.Request.IsReturnUrlDiscoverable) {
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
				"return_to could not be verified using RP discovery realm {0}.", e.Request.Realm));
		}
		switch (scenario) {
			case TestSupport.Scenarios.AutoApproval:
				// immediately approve
				e.Request.IsAuthenticated = true;
				break;
			case TestSupport.Scenarios.AutoApprovalAddFragment:
				e.Request.SetClaimedIdentifierFragment("frag");
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
