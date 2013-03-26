//-----------------------------------------------------------------------
// <copyright file="ExtensionsInteropHelperOPTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.Provider.Extensions;
	using NUnit.Framework;

	[TestFixture]
	public class ExtensionsInteropHelperOPTests : OpenIdTestBase {
		private AuthenticationRequest request;
		private IList<IExtensionMessage> extensions;

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			var op = this.CreateProvider();
			var rpRequest = new CheckIdRequest(Protocol.Default.Version, OPUri, DotNetOpenAuth.OpenId.AuthenticationRequestMode.Setup);
			rpRequest.ReturnTo = RPUri;
			this.extensions = rpRequest.Extensions;
			this.request = new AuthenticationRequest(op, rpRequest);
			this.request.IsAuthenticated = true;
		}

		/// <summary>
		/// Verifies no extensions appear as no extensions
		/// </summary>
		[Test]
		public async Task NoRequestedExtensions() {
			var sreg = ExtensionsInteropHelper.UnifyExtensionsAsSreg(this.request);
			Assert.IsNull(sreg);

			// Make sure we're still able to send an sreg response.
			// (not really a valid scenario, since OPs don't have public access
			// to directly create a response without a request.
			var sregResponse = new ClaimsResponse();
			this.request.AddResponseExtension(sregResponse);
			await ExtensionsInteropHelper.ConvertSregToMatchRequestAsync(this.request, CancellationToken.None);
			var extensions = await this.GetResponseExtensionsAsync();
			Assert.AreSame(sregResponse, extensions.Single());
		}

		[Test]
		public async Task NegativeResponse() {
			this.request.IsAuthenticated = false;
			await ExtensionsInteropHelper.ConvertSregToMatchRequestAsync(this.request, CancellationToken.None);
		}

		/// <summary>
		/// Verifies sreg coming in is seen as sreg.
		/// </summary>
		[Test]
		public async Task UnifyExtensionsAsSregWithSreg() {
			var sregInjected = new ClaimsRequest(DotNetOpenAuth.OpenId.Extensions.SimpleRegistration.Constants.TypeUris.Standard) {
				Nickname = DemandLevel.Request,
			};
			this.extensions.Add(sregInjected);
			var sreg = ExtensionsInteropHelper.UnifyExtensionsAsSreg(this.request);
			Assert.AreSame(sregInjected, sreg);
			Assert.AreEqual(DemandLevel.Request, sreg.Nickname);
			Assert.AreEqual(DemandLevel.NoRequest, sreg.FullName);

			var sregResponse = sreg.CreateResponse();
			this.request.AddResponseExtension(sregResponse);
			await ExtensionsInteropHelper.ConvertSregToMatchRequestAsync(this.request, CancellationToken.None);
			var extensions = await this.GetResponseExtensionsAsync();
			Assert.AreSame(sregResponse, extensions.Single());
		}

		/// <summary>
		/// Verifies AX coming in looks like sreg.
		/// </summary>
		[Test]
		public async Task UnifyExtensionsAsSregWithAX() {
			await this.ParameterizedAXTestAsync(AXAttributeFormats.AXSchemaOrg);
		}

		/// <summary>
		/// Verifies AX coming in looks like sreg.
		/// </summary>
		[Test]
		public async Task UnifyExtensionsAsSregWithAXSchemaOpenIdNet() {
			await this.ParameterizedAXTestAsync(AXAttributeFormats.SchemaOpenIdNet);
		}

		/// <summary>
		/// Verifies sreg and AX in one request has a preserved sreg request.
		/// </summary>
		[Test]
		public async Task UnifyExtensionsAsSregWithBothSregAndAX() {
			var sregInjected = new ClaimsRequest(DotNetOpenAuth.OpenId.Extensions.SimpleRegistration.Constants.TypeUris.Standard) {
				Nickname = DemandLevel.Request,
			};
			this.extensions.Add(sregInjected);
			var axInjected = new FetchRequest();
			axInjected.Attributes.AddOptional(WellKnownAttributes.Contact.Email);
			this.extensions.Add(axInjected);
			var sreg = ExtensionsInteropHelper.UnifyExtensionsAsSreg(this.request);
			Assert.AreSame(sregInjected, sreg);
			Assert.AreEqual(DemandLevel.Request, sreg.Nickname);
			Assert.AreEqual(DemandLevel.NoRequest, sreg.Email);

			var sregResponseInjected = sreg.CreateResponse();
			sregResponseInjected.Nickname = "andy";
			this.request.AddResponseExtension(sregResponseInjected);
			var axResponseInjected = new FetchResponse();
			axResponseInjected.Attributes.Add(WellKnownAttributes.Contact.Email, "a@b.com");
			this.request.AddResponseExtension(axResponseInjected);
			await ExtensionsInteropHelper.ConvertSregToMatchRequestAsync(this.request, CancellationToken.None);
			var extensions = await this.GetResponseExtensionsAsync();
			var sregResponse = extensions.OfType<ClaimsResponse>().Single();
			Assert.AreEqual("andy", sregResponse.Nickname);
			var axResponse = extensions.OfType<FetchResponse>().Single();
			Assert.AreEqual("a@b.com", axResponse.GetAttributeValue(WellKnownAttributes.Contact.Email));
		}

		private async Task<IList<IExtensionMessage>> GetResponseExtensionsAsync() {
			var response = (IProtocolMessageWithExtensions)await this.request.GetResponseAsync(CancellationToken.None);
			return response.Extensions;
		}

		private async Task ParameterizedAXTestAsync(AXAttributeFormats format) {
			var axInjected = new FetchRequest();
			axInjected.Attributes.AddOptional(ExtensionsInteropHelper.TransformAXFormatTestHook(WellKnownAttributes.Name.Alias, format));
			axInjected.Attributes.AddRequired(ExtensionsInteropHelper.TransformAXFormatTestHook(WellKnownAttributes.Name.FullName, format));
			this.extensions.Add(axInjected);
			var sreg = ExtensionsInteropHelper.UnifyExtensionsAsSreg(this.request);
			Assert.AreSame(sreg, this.request.GetExtension<ClaimsRequest>());
			Assert.AreEqual(DemandLevel.Request, sreg.Nickname);
			Assert.AreEqual(DemandLevel.Require, sreg.FullName);
			Assert.AreEqual(DemandLevel.NoRequest, sreg.Language);

			var sregResponse = sreg.CreateResponse();
			sregResponse.Nickname = "andy";
			this.request.AddResponseExtension(sregResponse);
			await ExtensionsInteropHelper.ConvertSregToMatchRequestAsync(this.request, CancellationToken.None);
			var extensions = await this.GetResponseExtensionsAsync();
			var axResponse = extensions.OfType<FetchResponse>().Single();
			Assert.AreEqual("andy", axResponse.GetAttributeValue(ExtensionsInteropHelper.TransformAXFormatTestHook(WellKnownAttributes.Name.Alias, format)));
		}
	}
}
