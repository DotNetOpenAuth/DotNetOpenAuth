//-----------------------------------------------------------------------
// <copyright file="ExtensionsBindingElementTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using DotNetOpenAuth.Test.OpenId.Extensions;
	using NUnit.Framework;
	using Validation;

	[TestFixture]
	public class ExtensionsBindingElementTests : OpenIdTestBase {
		private StandardOpenIdExtensionFactory factory;
		private ExtensionsBindingElement rpElement;
		private IProtocolMessageWithExtensions request;

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			this.factory = new StandardOpenIdExtensionFactory();
			this.factory.RegisterExtension(MockOpenIdExtension.Factory);
			this.rpElement = new ExtensionsBindingElementRelyingParty(this.factory, new RelyingPartySecuritySettings());
			this.rpElement.Channel = new TestChannel(this.MessageDescriptions);
			this.request = new SignedResponseRequest(Protocol.Default.Version, OpenIdTestBase.OPUri, AuthenticationRequestMode.Immediate);
		}

		[Test]
		public async Task RoundTripFullStackTest() {
			IOpenIdMessageExtension request = new MockOpenIdExtension("requestPart", "requestData");
			IOpenIdMessageExtension response = new MockOpenIdExtension("responsePart", "responseData");
			await this.RoundtripAsync(
				Protocol.Default,
				new IOpenIdMessageExtension[] { request },
				new IOpenIdMessageExtension[] { response });
		}

		[Test]
		public void ExtensionFactory() {
			Assert.AreSame(this.factory, this.rpElement.ExtensionFactory);
		}

		[Test]
		public async Task PrepareMessageForSendingNull() {
			Assert.IsNull(await this.rpElement.ProcessOutgoingMessageAsync(null, CancellationToken.None));
		}

		/// <summary>
		/// Verifies that false is returned when a non-extendable message is sent.
		/// </summary>
		[Test]
		public async Task PrepareMessageForSendingNonExtendableMessage() {
			IProtocolMessage request = new AssociateDiffieHellmanRequest(Protocol.Default.Version, OpenIdTestBase.OPUri);
			Assert.IsNull(await this.rpElement.ProcessOutgoingMessageAsync(request, CancellationToken.None));
		}

		[Test]
		public async Task PrepareMessageForSending() {
			this.request.Extensions.Add(new MockOpenIdExtension("part", "extra"));
			Assert.IsNotNull(await this.rpElement.ProcessOutgoingMessageAsync(this.request, CancellationToken.None));

			string alias = GetAliases(this.request.ExtraData).Single();
			Assert.AreEqual(MockOpenIdExtension.MockTypeUri, this.request.ExtraData["openid.ns." + alias]);
			Assert.AreEqual("part", this.request.ExtraData["openid." + alias + ".Part"]);
			Assert.AreEqual("extra", this.request.ExtraData["openid." + alias + ".data"]);
		}

		[Test]
		public async Task PrepareMessageForReceiving() {
			this.request.ExtraData["openid.ns.mock"] = MockOpenIdExtension.MockTypeUri;
			this.request.ExtraData["openid.mock.Part"] = "part";
			this.request.ExtraData["openid.mock.data"] = "extra";
			Assert.IsNotNull(await this.rpElement.ProcessIncomingMessageAsync(this.request, CancellationToken.None));
			MockOpenIdExtension ext = this.request.Extensions.OfType<MockOpenIdExtension>().Single();
			Assert.AreEqual("part", ext.Part);
			Assert.AreEqual("extra", ext.Data);
		}

		/// <summary>
		/// Verifies that extension responses are included in the OP's signature.
		/// </summary>
		[Test]
		public async Task ExtensionResponsesAreSigned() {
			Protocol protocol = Protocol.Default;
			var op = this.CreateProvider();
			IndirectSignedResponse response = this.CreateResponseWithExtensions(protocol);
			await op.Channel.PrepareResponseAsync(response);
			ITamperResistantOpenIdMessage signedResponse = response;
			string extensionAliasKey = signedResponse.ExtraData.Single(kv => kv.Value == MockOpenIdExtension.MockTypeUri).Key;
			Assert.IsTrue(extensionAliasKey.StartsWith("openid.ns."));
			string extensionAlias = extensionAliasKey.Substring("openid.ns.".Length);

			// Make sure that the extension members and the alias=namespace declaration are all signed.
			Assert.IsNotNull(signedResponse.SignedParameterOrder);
			string[] signedParameters = signedResponse.SignedParameterOrder.Split(',');
			Assert.IsTrue(signedParameters.Contains(extensionAlias + ".Part"));
			Assert.IsTrue(signedParameters.Contains(extensionAlias + ".data"));
			Assert.IsTrue(signedParameters.Contains("ns." + extensionAlias));
		}

		/// <summary>
		/// Verifies that unsigned extension responses (where any or all fields are unsigned) are ignored.
		/// </summary>
		[Test]
		public async Task ExtensionsAreIdentifiedAsSignedOrUnsigned() {
			Protocol protocol = Protocol.Default;
			var opStore = new MemoryCryptoKeyAndNonceStore();
			int rpStep = 0;

			Handle(RPUri).By(
				async req => {
					var rp = new OpenIdRelyingParty(new MemoryCryptoKeyAndNonceStore(), this.HostFactories);
					RegisterMockExtension(rp.Channel);

					switch (++rpStep) {
						case 1:
							var response = await rp.Channel.ReadFromRequestAsync<IndirectSignedResponse>(req, CancellationToken.None);
							Assert.AreEqual(1, response.SignedExtensions.Count(), "Signed extension should have been received.");
							Assert.AreEqual(0, response.UnsignedExtensions.Count(), "No unsigned extension should be present.");
							break;
						case 2:
							response = await rp.Channel.ReadFromRequestAsync<IndirectSignedResponse>(req, CancellationToken.None);
							Assert.AreEqual(0, response.SignedExtensions.Count(), "No signed extension should have been received.");
							Assert.AreEqual(1, response.UnsignedExtensions.Count(), "Unsigned extension should have been received.");
							break;

						default:
							throw Assumes.NotReachable();
					}

					return new HttpResponseMessage();
				});
			Handle(OPUri).By(
				async req => {
					var op = new OpenIdProvider(opStore, this.HostFactories);
					return await AutoProviderActionAsync(op, req, CancellationToken.None);
				});

			{
				var op = new OpenIdProvider(opStore, this.HostFactories);
				RegisterMockExtension(op.Channel);
				var redirectingResponse = await op.Channel.PrepareResponseAsync(this.CreateResponseWithExtensions(protocol));
				using (var httpClient = this.HostFactories.CreateHttpClient()) {
					using (var response = await httpClient.GetAsync(redirectingResponse.Headers.Location)) {
						response.EnsureSuccessStatusCode();
					}
				}

				op.SecuritySettings.SignOutgoingExtensions = false;
				redirectingResponse = await op.Channel.PrepareResponseAsync(this.CreateResponseWithExtensions(protocol));
				using (var httpClient = this.HostFactories.CreateHttpClient()) {
					using (var response = await httpClient.GetAsync(redirectingResponse.Headers.Location)) {
						response.EnsureSuccessStatusCode();
					}
				}
			}
		}

		/// <summary>
		/// Verifies that two extensions with the same TypeURI cannot be applied to the same message.
		/// </summary>
		/// <remarks>
		/// OpenID Authentication 2.0 section 12 states that
		/// "A namespace MUST NOT be assigned more than one alias in the same message".
		/// </remarks>
		[Test]
		public async Task TwoExtensionsSameTypeUri() {
			IOpenIdMessageExtension request1 = new MockOpenIdExtension("requestPart1", "requestData1");
			IOpenIdMessageExtension request2 = new MockOpenIdExtension("requestPart2", "requestData2");
			try {
				await this.RoundtripAsync(
					Protocol.Default,
					new IOpenIdMessageExtension[] { request1, request2 },
					new IOpenIdMessageExtension[0]);
				Assert.Fail("Expected ProtocolException not thrown.");
			} catch (ProtocolException) {
				// success
			}
		}

		private static IEnumerable<string> GetAliases(IDictionary<string, string> extraData) {
			Regex regex = new Regex(@"^openid\.ns\.(\w+)");
			return from key in extraData.Keys
				   let m = regex.Match(key)
				   where m.Success
				   select m.Groups[1].Value;
		}

		private static void RegisterMockExtension(Channel channel) {
			Requires.NotNull(channel, "channel");

			ExtensionTestUtilities.RegisterExtension(channel, MockOpenIdExtension.Factory);
		}

		/// <summary>
		/// Creates a response message with one extensions.
		/// </summary>
		/// <param name="protocol">The protocol to construct the message with.</param>
		/// <returns>The message ready to send from OP to RP.</returns>
		private IndirectSignedResponse CreateResponseWithExtensions(Protocol protocol) {
			Requires.NotNull(protocol, "protocol");

			var response = new IndirectSignedResponse(protocol.Version, RPUri);
			response.ProviderEndpoint = OPUri;
			response.Extensions.Add(new MockOpenIdExtension("pv", "ev"));
			return response;
		}
	}
}
