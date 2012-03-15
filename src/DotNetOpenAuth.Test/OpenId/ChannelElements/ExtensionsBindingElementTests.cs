//-----------------------------------------------------------------------
// <copyright file="ExtensionsBindingElementTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text.RegularExpressions;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using DotNetOpenAuth.Test.OpenId.Extensions;
	using NUnit.Framework;

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
		public void RoundTripFullStackTest() {
			IOpenIdMessageExtension request = new MockOpenIdExtension("requestPart", "requestData");
			IOpenIdMessageExtension response = new MockOpenIdExtension("responsePart", "responseData");
			ExtensionTestUtilities.Roundtrip(
				Protocol.Default,
				new IOpenIdMessageExtension[] { request },
				new IOpenIdMessageExtension[] { response });
		}

		[Test]
		public void ExtensionFactory() {
			Assert.AreSame(this.factory, this.rpElement.ExtensionFactory);
		}

		[Test]
		public void PrepareMessageForSendingNull() {
			Assert.IsNull(this.rpElement.ProcessOutgoingMessage(null));
		}

		/// <summary>
		/// Verifies that false is returned when a non-extendable message is sent.
		/// </summary>
		[Test]
		public void PrepareMessageForSendingNonExtendableMessage() {
			IProtocolMessage request = new AssociateDiffieHellmanRequest(Protocol.Default.Version, OpenIdTestBase.OPUri);
			Assert.IsNull(this.rpElement.ProcessOutgoingMessage(request));
		}

		[Test]
		public void PrepareMessageForSending() {
			this.request.Extensions.Add(new MockOpenIdExtension("part", "extra"));
			Assert.IsNotNull(this.rpElement.ProcessOutgoingMessage(this.request));

			string alias = GetAliases(this.request.ExtraData).Single();
			Assert.AreEqual(MockOpenIdExtension.MockTypeUri, this.request.ExtraData["openid.ns." + alias]);
			Assert.AreEqual("part", this.request.ExtraData["openid." + alias + ".Part"]);
			Assert.AreEqual("extra", this.request.ExtraData["openid." + alias + ".data"]);
		}

		[Test]
		public void PrepareMessageForReceiving() {
			this.request.ExtraData["openid.ns.mock"] = MockOpenIdExtension.MockTypeUri;
			this.request.ExtraData["openid.mock.Part"] = "part";
			this.request.ExtraData["openid.mock.data"] = "extra";
			Assert.IsNotNull(this.rpElement.ProcessIncomingMessage(this.request));
			MockOpenIdExtension ext = this.request.Extensions.OfType<MockOpenIdExtension>().Single();
			Assert.AreEqual("part", ext.Part);
			Assert.AreEqual("extra", ext.Data);
		}

		/// <summary>
		/// Verifies that extension responses are included in the OP's signature.
		/// </summary>
		[Test]
		public void ExtensionResponsesAreSigned() {
			Protocol protocol = Protocol.Default;
			var op = this.CreateProvider();
			IndirectSignedResponse response = this.CreateResponseWithExtensions(protocol);
			op.Channel.PrepareResponse(response);
			ITamperResistantOpenIdMessage signedResponse = (ITamperResistantOpenIdMessage)response;
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
		public void ExtensionsAreIdentifiedAsSignedOrUnsigned() {
			Protocol protocol = Protocol.Default;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					RegisterMockExtension(rp.Channel);
					var response = rp.Channel.ReadFromRequest<IndirectSignedResponse>();
					Assert.AreEqual(1, response.SignedExtensions.Count(), "Signed extension should have been received.");
					Assert.AreEqual(0, response.UnsignedExtensions.Count(), "No unsigned extension should be present.");
					response = rp.Channel.ReadFromRequest<IndirectSignedResponse>();
					Assert.AreEqual(0, response.SignedExtensions.Count(), "No signed extension should have been received.");
					Assert.AreEqual(1, response.UnsignedExtensions.Count(), "Unsigned extension should have been received.");
				},
				op => {
					RegisterMockExtension(op.Channel);
					op.Channel.Respond(CreateResponseWithExtensions(protocol));
					op.Respond(op.GetRequest()); // check_auth
					op.SecuritySettings.SignOutgoingExtensions = false;
					op.Channel.Respond(CreateResponseWithExtensions(protocol));
					op.Respond(op.GetRequest()); // check_auth
				});
			coordinator.Run();
		}

		/// <summary>
		/// Verifies that two extensions with the same TypeURI cannot be applied to the same message.
		/// </summary>
		/// <remarks>
		/// OpenID Authentication 2.0 section 12 states that
		/// "A namespace MUST NOT be assigned more than one alias in the same message".
		/// </remarks>
		[Test]
		public void TwoExtensionsSameTypeUri() {
			IOpenIdMessageExtension request1 = new MockOpenIdExtension("requestPart1", "requestData1");
			IOpenIdMessageExtension request2 = new MockOpenIdExtension("requestPart2", "requestData2");
			try {
				ExtensionTestUtilities.Roundtrip(
					Protocol.Default,
					new IOpenIdMessageExtension[] { request1, request2 },
					new IOpenIdMessageExtension[0]);
				Assert.Fail("Expected ProtocolException not thrown.");
			} catch (AssertionException ex) {
				Assert.IsInstanceOf<ProtocolException>(ex.InnerException);
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

			IndirectSignedResponse response = new IndirectSignedResponse(protocol.Version, RPUri);
			response.ProviderEndpoint = OPUri;
			response.Extensions.Add(new MockOpenIdExtension("pv", "ev"));
			return response;
		}
	}
}
