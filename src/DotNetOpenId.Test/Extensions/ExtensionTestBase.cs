using System;
using System.Collections.Generic;
using DotNetOpenId.Extensions;
using DotNetOpenId.Extensions.AttributeExchange;
using DotNetOpenId.Extensions.ProviderAuthenticationPolicy;
using DotNetOpenId.Extensions.SimpleRegistration;
using DotNetOpenId.RelyingParty;
using NUnit.Framework;
using OPRequest = DotNetOpenId.Provider.IAuthenticationRequest;
using SregDemandLevel = DotNetOpenId.Extensions.SimpleRegistration.DemandLevel;
using PapeConstants = DotNetOpenId.Extensions.ProviderAuthenticationPolicy.Constants;

namespace DotNetOpenId.Test.Extensions {
	public class ExtensionTestBase {
		protected const ProtocolVersion Version = ProtocolVersion.V20;
		Dictionary<string, AttributeValues> storedAttributes;

		[SetUp]
		public virtual void Setup() {
			storedAttributes = new Dictionary<string, AttributeValues>();
		}

		[TearDown]
		public virtual void TearDown() {
			Mocks.MockHttpRequest.Reset();
		}

		protected T ParameterizedTest<T>(TestSupport.Scenarios scenario, ProtocolVersion version, IExtensionRequest extension)
			where T : IExtensionResponse, new() {
			var rpRequest = TestSupport.CreateRelyingPartyRequest(false, scenario, version);
			if (extension != null)
				rpRequest.AddExtension(extension);

			var response = TestSupport.CreateRelyingPartyResponseThroughProvider(rpRequest, request => {
				TestSupport.SetAuthenticationFromScenario(scenario, request);
				ExtensionsResponder(request);
			});
			Assert.AreEqual(AuthenticationStatus.Authenticated, response.Status);
			return response.GetExtension<T>();
		}

		const string nicknameTypeUri = WellKnownAttributes.Name.Alias;
		const string emailTypeUri = WellKnownAttributes.Contact.Email;

		private void ExtensionsResponder(OPRequest request) {
			var sregRequest = request.GetExtension<ClaimsRequest>();
			var sregResponse = sregRequest != null ? sregRequest.CreateResponse() : null;
			var aeFetchRequest = request.GetExtension<FetchRequest>();
			var aeFetchResponse = new FetchResponse();
			var aeStoreRequest = request.GetExtension<StoreRequest>();
			var aeStoreResponse = new StoreResponse();
			var papeRequest = request.GetExtension<PolicyRequest>();
			var papeResponse = new PolicyResponse();

			TestSupport.Scenarios scenario = (TestSupport.Scenarios)Enum.Parse(typeof(TestSupport.Scenarios),
				new Uri(request.LocalIdentifier).AbsolutePath.TrimStart('/'));
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
						if (papeRequest.PreferredAuthLevelTypes.Contains(PapeConstants.AuthenticationLevels.NistTypeUri)) {
							papeResponse.NistAssuranceLevel = NistAssuranceLevel.Level1;
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
	}
}
