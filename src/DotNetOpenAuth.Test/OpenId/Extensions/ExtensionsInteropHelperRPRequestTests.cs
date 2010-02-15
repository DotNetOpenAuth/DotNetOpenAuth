//-----------------------------------------------------------------------
// <copyright file="ExtensionsInteropHelperRPRequestTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System.Linq;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ExtensionsInteropHelperRPRequestTests : OpenIdTestBase {
		private AuthenticationRequest authReq;
		private ClaimsRequest sreg;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			var rp = CreateRelyingParty(true);
			Identifier identifier = this.GetMockIdentifier(ProtocolVersion.V20);
			this.authReq = (AuthenticationRequest)rp.CreateRequest(identifier, RPRealmUri, RPUri);
			this.sreg = new ClaimsRequest {
				Nickname = DemandLevel.Request,
				FullName = DemandLevel.Request,
				BirthDate = DemandLevel.Request,
				Email = DemandLevel.Require,
				Country = DemandLevel.Request,
				PostalCode = DemandLevel.Request,
				Gender = DemandLevel.Request,
				Language = DemandLevel.Request,
				TimeZone = DemandLevel.Request,
			};
		}

		/// <summary>
		/// Verifies that without an Sreg extension to copy from, no AX extension request is added.
		/// </summary>
		[TestMethod]
		public void SpreadSregToAXNoExtensions() {
			ExtensionsInteropHelper.SpreadSregToAX(this.authReq, AXAttributeFormats.AXSchemaOrg);
			Assert.AreEqual(0, this.authReq.AppliedExtensions.Count());
		}

		/// <summary>
		/// Verifies that Sreg requests are correctly copied to axschema.org AX requests.
		/// </summary>
		[TestMethod]
		public void SpreadSregToAXBasic() {
			this.authReq.AddExtension(this.sreg);
			ExtensionsInteropHelper.SpreadSregToAX(this.authReq, AXAttributeFormats.AXSchemaOrg);
			var ax = this.authReq.AppliedExtensions.OfType<FetchRequest>().Single();
			Assert.IsFalse(ax.Attributes[WellKnownAttributes.Name.Alias].IsRequired);
			Assert.IsFalse(ax.Attributes[WellKnownAttributes.Name.FullName].IsRequired);
			Assert.IsFalse(ax.Attributes[WellKnownAttributes.BirthDate.WholeBirthDate].IsRequired);
			Assert.IsTrue(ax.Attributes[WellKnownAttributes.Contact.Email].IsRequired);
			Assert.IsFalse(ax.Attributes[WellKnownAttributes.Contact.HomeAddress.Country].IsRequired);
			Assert.IsFalse(ax.Attributes[WellKnownAttributes.Contact.HomeAddress.PostalCode].IsRequired);
			Assert.IsFalse(ax.Attributes[WellKnownAttributes.Person.Gender].IsRequired);
			Assert.IsFalse(ax.Attributes[WellKnownAttributes.Preferences.Language].IsRequired);
			Assert.IsFalse(ax.Attributes[WellKnownAttributes.Preferences.TimeZone].IsRequired);
		}

		/// <summary>
		/// Verifies that sreg can spread to multiple AX schemas.
		/// </summary>
		[TestMethod]
		public void SpreadSregToAxMultipleSchemas() {
			this.authReq.AddExtension(this.sreg);
			ExtensionsInteropHelper.SpreadSregToAX(this.authReq, AXAttributeFormats.AXSchemaOrg | AXAttributeFormats.SchemaOpenIdNet);
			var ax = this.authReq.AppliedExtensions.OfType<FetchRequest>().Single();
			Assert.IsTrue(ax.Attributes.Contains(WellKnownAttributes.Name.Alias));
			Assert.IsTrue(ax.Attributes.Contains(ExtensionsInteropHelper_Accessor.TransformAXFormat(WellKnownAttributes.Name.Alias, AXAttributeFormats.SchemaOpenIdNet)));
			Assert.IsFalse(ax.Attributes.Contains(ExtensionsInteropHelper_Accessor.TransformAXFormat(WellKnownAttributes.Name.Alias, AXAttributeFormats.OpenIdNetSchema)));
		}

		/// <summary>
		/// Verifies no spread if the OP advertises sreg support.
		/// </summary>
		[TestMethod]
		public void SpreadSregToAxNoOpIfOPSupportsSreg() {
			this.authReq.AddExtension(this.sreg);
			this.InjectAdvertisedTypeUri(DotNetOpenAuth.OpenId.Extensions.SimpleRegistration.Constants.sreg_ns);
			ExtensionsInteropHelper.SpreadSregToAX(this.authReq, AXAttributeFormats.All);
			Assert.IsFalse(this.authReq.AppliedExtensions.OfType<FetchRequest>().Any());
		}

		/// <summary>
		/// Verifies a targeted AX request if the OP advertises a recognized type URI format.
		/// </summary>
		[TestMethod]
		public void SpreadSregToAxTargetedAtOPFormat() {
			this.authReq.AddExtension(this.sreg);
			this.InjectAdvertisedTypeUri(WellKnownAttributes.Name.FullName);
			ExtensionsInteropHelper.SpreadSregToAX(this.authReq, AXAttributeFormats.OpenIdNetSchema);
			var ax = this.authReq.AppliedExtensions.OfType<FetchRequest>().Single();
			Assert.IsFalse(ax.Attributes.Contains(ExtensionsInteropHelper_Accessor.TransformAXFormat(WellKnownAttributes.Contact.Email, AXAttributeFormats.OpenIdNetSchema)));
			Assert.IsTrue(ax.Attributes.Contains(WellKnownAttributes.Contact.Email));
		}

		/// <summary>
		/// Verifies that TransformAXFormat correctly translates AX schema Type URIs.
		/// </summary>
		[TestMethod]
		public void TransformAXFormatTest() {
			Assert.AreEqual(WellKnownAttributes.Name.Alias, ExtensionsInteropHelper_Accessor.TransformAXFormat(WellKnownAttributes.Name.Alias, AXAttributeFormats.AXSchemaOrg));
			Assert.AreEqual("http://schema.openid.net/namePerson/friendly", ExtensionsInteropHelper_Accessor.TransformAXFormat(WellKnownAttributes.Name.Alias, AXAttributeFormats.SchemaOpenIdNet));
			Assert.AreEqual("http://openid.net/schema/namePerson/friendly", ExtensionsInteropHelper_Accessor.TransformAXFormat(WellKnownAttributes.Name.Alias, AXAttributeFormats.OpenIdNetSchema));
		}

		/// <summary>
		/// Injects the advertised type URI into the list of advertised services for the authentication request.
		/// </summary>
		/// <param name="typeUri">The type URI.</param>
		private void InjectAdvertisedTypeUri(string typeUri) {
			var serviceEndpoint = ServiceEndpoint_Accessor.AttachShadow(((ServiceEndpoint)this.authReq.Provider));
			serviceEndpoint.ProviderDescription = ProviderEndpointDescription_Accessor.AttachShadow(
				new ProviderEndpointDescription(
					serviceEndpoint.ProviderDescription.Endpoint,
					serviceEndpoint.ProviderDescription.Capabilities.Concat(new[] { typeUri })));
		}
	}
}
