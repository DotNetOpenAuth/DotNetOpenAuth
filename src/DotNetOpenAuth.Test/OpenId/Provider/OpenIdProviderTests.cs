//-----------------------------------------------------------------------
// <copyright file="OpenIdProviderTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Hosting;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class OpenIdProviderTests : OpenIdTestBase {
		private OpenIdProvider provider;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.provider = this.CreateProvider();
		}

		/// <summary>
		/// Verifies that the constructor throws an exception if the app store is null.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNull() {
			new OpenIdProvider(null);
		}

		/// <summary>
		/// Verifies that the SecuritySettings property throws when set to null.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SecuritySettingsSetNull() {
			this.provider.SecuritySettings = null;
		}

		/// <summary>
		/// Verifies the SecuritySettings property can be set to a new instance.
		/// </summary>
		[TestMethod]
		public void SecuritySettings() {
			var newSettings = new ProviderSecuritySettings();
			this.provider.SecuritySettings = newSettings;
			Assert.AreSame(newSettings, this.provider.SecuritySettings);
		}

		[TestMethod]
		public void ExtensionFactories() {
			var factories = this.provider.ExtensionFactories;
			Assert.IsNotNull(factories);
			Assert.AreEqual(1, factories.Count);
			Assert.IsInstanceOfType(factories[0], typeof(StandardOpenIdExtensionFactory));
		}

		/// <summary>
		/// Verifies the Channel property.
		/// </summary>
		[TestMethod]
		public void ChannelGetter() {
			Assert.IsNotNull(this.provider.Channel);
		}

		/// <summary>
		/// Verifies the GetRequest method throws outside an HttpContext.
		/// </summary>
		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void GetRequestNoContext() {
			this.provider.GetRequest();
		}

		/// <summary>
		/// Verifies GetRequest throws on null input.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void GetRequestNull() {
			this.provider.GetRequest(null);
		}

		/// <summary>
		/// Verifies that GetRequest correctly returns the right messages.
		/// </summary>
		[TestMethod]
		public void GetRequest() {
			HttpRequestInfo httpInfo = new HttpRequestInfo();
			httpInfo.UrlBeforeRewriting = new Uri("http://someUri");
			Assert.IsNull(this.provider.GetRequest(httpInfo), "An irrelevant request should return null.");
			var providerDescription = new ProviderEndpointDescription(OpenIdTestBase.OPUri, Protocol.Default.Version);

			// Test some non-empty request scenario.
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					rp.Channel.Request(AssociateRequest.Create(rp.SecuritySettings, providerDescription));
				},
				op => {
					IRequest request = op.GetRequest();
					Assert.IsInstanceOfType(request, typeof(AutoResponsiveRequest));
					op.SendResponse(request);
				});
			coordinator.Run();
		}

		[TestMethod]
		public void BadRequestsGenerateValidErrorResponses() {
			var coordinator = new OpenIdCoordinator(
				rp => {
					var nonOpenIdMessage = new Mocks.TestDirectedMessage();
					nonOpenIdMessage.Recipient = OPUri;
					nonOpenIdMessage.HttpMethods = HttpDeliveryMethods.PostRequest;
					MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired, nonOpenIdMessage);
					var response = rp.Channel.Request<DirectErrorResponse>(nonOpenIdMessage);
					Assert.IsNotNull(response.ErrorMessage);
					Assert.AreEqual(Protocol.Default.Version, response.Version);
				},
				AutoProvider);

			coordinator.Run();
		}

		[TestMethod]
		public void BadRequestsGenerateValidErrorResponsesHosted() {
			using (AspNetHost host = AspNetHost.CreateHost(TestWebDirectory)) {
				Uri opEndpoint = new Uri(host.BaseUri, "/OpenIdProviderEndpoint.ashx");
				var rp = new OpenIdRelyingParty(null);
				var nonOpenIdMessage = new Mocks.TestDirectedMessage();
				nonOpenIdMessage.Recipient = opEndpoint;
				nonOpenIdMessage.HttpMethods = HttpDeliveryMethods.PostRequest;
				MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired, nonOpenIdMessage);
				var response = rp.Channel.Request<DirectErrorResponse>(nonOpenIdMessage);
				Assert.IsNotNull(response.ErrorMessage);
			}
		}
	}
}
