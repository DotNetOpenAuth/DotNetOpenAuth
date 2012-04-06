//-----------------------------------------------------------------------
// <copyright file="OpenIdProviderTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using System;
	using System.IO;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Hosting;
	using NUnit.Framework;

	[TestFixture]
	public class OpenIdProviderTests : OpenIdTestBase {
		private OpenIdProvider provider;

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			this.provider = this.CreateProvider();
		}

		/// <summary>
		/// Verifies that the constructor throws an exception if the app store is null.
		/// </summary>
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNull() {
			new OpenIdProvider(null);
		}

		/// <summary>
		/// Verifies that the SecuritySettings property throws when set to null.
		/// </summary>
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SecuritySettingsSetNull() {
			this.provider.SecuritySettings = null;
		}

		/// <summary>
		/// Verifies the SecuritySettings property can be set to a new instance.
		/// </summary>
		[Test]
		public void SecuritySettings() {
			var newSettings = new ProviderSecuritySettings();
			this.provider.SecuritySettings = newSettings;
			Assert.AreSame(newSettings, this.provider.SecuritySettings);
		}

		[Test]
		public void ExtensionFactories() {
			var factories = this.provider.ExtensionFactories;
			Assert.IsNotNull(factories);
			Assert.AreEqual(1, factories.Count);
			Assert.IsInstanceOf<StandardOpenIdExtensionFactory>(factories[0]);
		}

		/// <summary>
		/// Verifies the Channel property.
		/// </summary>
		[Test]
		public void ChannelGetter() {
			Assert.IsNotNull(this.provider.Channel);
		}

		/// <summary>
		/// Verifies the GetRequest method throws outside an HttpContext.
		/// </summary>
		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void GetRequestNoContext() {
			HttpContext.Current = null;
			this.provider.GetRequest();
		}

		/// <summary>
		/// Verifies GetRequest throws on null input.
		/// </summary>
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void GetRequestNull() {
			this.provider.GetRequest(null);
		}

		/// <summary>
		/// Verifies that GetRequest correctly returns the right messages.
		/// </summary>
		[Test]
		public void GetRequest() {
			var httpInfo = new HttpRequestInfo("GET", new Uri("http://someUri"));
			Assert.IsNull(this.provider.GetRequest(httpInfo), "An irrelevant request should return null.");
			var providerDescription = new ProviderEndpointDescription(OPUri, Protocol.Default.Version);

			// Test some non-empty request scenario.
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					rp.Channel.Request(AssociateRequestRelyingParty.Create(rp.SecuritySettings, providerDescription));
				},
				op => {
					IRequest request = op.GetRequest();
					Assert.IsInstanceOf<AutoResponsiveRequest>(request);
					op.Respond(request);
				});
			coordinator.Run();
		}

		[Test]
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

		[Test, Category("HostASPNET")]
		public void BadRequestsGenerateValidErrorResponsesHosted() {
			try {
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
			} catch (FileNotFoundException ex) {
				Assert.Inconclusive("Unable to execute hosted ASP.NET tests because {0} could not be found.  {1}", ex.FileName, ex.FusionLog);
			}
		}
	}
}
