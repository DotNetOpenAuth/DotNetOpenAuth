//-----------------------------------------------------------------------
// <copyright file="OpenIdProviderTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using System;
	using System.IO;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
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
		public async Task GetRequestNoContext() {
			HttpContext.Current = null;
			await this.provider.GetRequestAsync();
		}

		/// <summary>
		/// Verifies GetRequest throws on null input.
		/// </summary>
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public async Task GetRequestNull() {
			await this.provider.GetRequestAsync((HttpRequestMessage)null);
		}

		/// <summary>
		/// Verifies that GetRequest correctly returns the right messages.
		/// </summary>
		[Test]
		public async Task GetRequest() {
			var httpInfo = new HttpRequestMessage(HttpMethod.Get, "http://someUri");
			Assert.IsNull(await this.provider.GetRequestAsync(httpInfo), "An irrelevant request should return null.");
			var providerDescription = new ProviderEndpointDescription(OPUri, Protocol.Default.Version);

			// Test some non-empty request scenario.
			HandleProvider(
				async (op, req) => {
					IRequest request = await op.GetRequestAsync(req);
					Assert.IsInstanceOf<AutoResponsiveRequest>(request);
					return await op.PrepareResponseAsync(request);
				});
			var rp = this.CreateRelyingParty();
			await rp.Channel.RequestAsync(AssociateRequestRelyingParty.Create(rp.SecuritySettings, providerDescription), CancellationToken.None);
		}

		[Test]
		public async Task BadRequestsGenerateValidErrorResponses() {
			this.RegisterAutoProvider();
			var rp = this.CreateRelyingParty();
			var nonOpenIdMessage = new Mocks.TestDirectedMessage {
				Recipient = OPUri,
				HttpMethods = HttpDeliveryMethods.PostRequest
			};
			MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired, nonOpenIdMessage);
			var response = await rp.Channel.RequestAsync<DirectErrorResponse>(nonOpenIdMessage, CancellationToken.None);
			Assert.IsNotNull(response.ErrorMessage);
			Assert.AreEqual(Protocol.Default.Version, response.Version);
		}
	}
}
