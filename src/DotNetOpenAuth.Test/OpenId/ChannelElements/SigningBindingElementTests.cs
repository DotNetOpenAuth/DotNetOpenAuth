//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.Test.Mocks;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class SigningBindingElementTests : OpenIdTestBase {
		/// <summary>
		/// Verifies that the signatures generated match Known Good signatures.
		/// </summary>
		[Test]
		public async Task SignaturesMatchKnownGood() {
			Protocol protocol = Protocol.V20;
			var settings = new ProviderSecuritySettings();
			var cryptoStore = new MemoryCryptoKeyStore();
			byte[] associationSecret = Convert.FromBase64String("rsSwv1zPWfjPRQU80hciu8FPDC+GONAMJQ/AvSo1a2M=");
			string handle = "mock";
			cryptoStore.StoreKey(ProviderAssociationKeyStorage.SharedAssociationBucket, handle, new CryptoKey(associationSecret, DateTime.UtcNow.AddDays(1)));

			var store = new ProviderAssociationKeyStorage(cryptoStore);
			SigningBindingElement signer = new ProviderSigningBindingElement(store, settings);
			signer.Channel = new TestChannel(this.MessageDescriptions);

			IndirectSignedResponse message = new IndirectSignedResponse(protocol.Version, new Uri("http://rp"));
			ITamperResistantOpenIdMessage signedMessage = message;
			message.ProviderEndpoint = new Uri("http://provider");
			signedMessage.UtcCreationDate = DateTime.Parse("1/1/2009");
			signedMessage.AssociationHandle = handle;
			Assert.IsNotNull(await signer.ProcessOutgoingMessageAsync(message, CancellationToken.None));
			Assert.AreEqual("o9+uN7qTaUS9v0otbHTuNAtbkpBm14+es9QnNo6IHD4=", signedMessage.Signature);
		}

		/// <summary>
		/// Verifies that all parameters in ExtraData in signed responses are signed.
		/// </summary>
		[Test]
		public async Task SignedResponsesIncludeExtraDataInSignature() {
			Protocol protocol = Protocol.Default;
			SigningBindingElement sbe = new ProviderSigningBindingElement(new ProviderAssociationHandleEncoder(new MemoryCryptoKeyStore()), new ProviderSecuritySettings());
			sbe.Channel = new TestChannel(this.MessageDescriptions);
			IndirectSignedResponse response = new IndirectSignedResponse(protocol.Version, RPUri);
			response.ReturnTo = RPUri;
			response.ProviderEndpoint = OPUri;

			response.ExtraData["someunsigned"] = "value";
			response.ExtraData["openid.somesigned"] = "value";

			Assert.IsNotNull(await sbe.ProcessOutgoingMessageAsync(response, CancellationToken.None));
			ITamperResistantOpenIdMessage signedResponse = (ITamperResistantOpenIdMessage)response;

			// Make sure that the extra parameters are signed.
			// Since the signing algorithm only allows for signing parameters that start with
			// 'openid.', other parameters should not be signed.
			Assert.IsNotNull(signedResponse.SignedParameterOrder);
			string[] signedParameters = signedResponse.SignedParameterOrder.Split(',');
			Assert.IsTrue(signedParameters.Contains("somesigned"));
			Assert.IsFalse(signedParameters.Contains("someunsigned"));
		}

		/// <summary>
		/// Regression test for bug #45 (https://github.com/AArnott/dotnetopenid/issues/45)
		/// </summary>
		[Test, ExpectedException(typeof(ProtocolException))]
		public async Task MissingSignedParameter() {
			var cryptoStore = new MemoryCryptoKeyStore();
			byte[] associationSecret = Convert.FromBase64String("rsSwv1zPWfjPRQU80hciu8FPDC+GONAMJQ/AvSo1a2M=");
			string handle = "{634477555066085461}{TTYcIg==}{32}";
			cryptoStore.StoreKey(ProviderAssociationKeyStorage.PrivateAssociationBucket, handle, new CryptoKey(associationSecret, DateTime.UtcNow.AddDays(1)));

			var signer = new ProviderSigningBindingElement(new ProviderAssociationKeyStorage(cryptoStore), new ProviderSecuritySettings());
			var testChannel = new TestChannel(new OpenIdProviderMessageFactory(), new IChannelBindingElement[0], this.HostFactories);
			signer.Channel = testChannel;

			var buggyRPMessage = new Dictionary<string, string>() {
				{ "openid.assoc_handle", "{634477555066085461}{TTYcIg==}{32}" },
				{ "openid.claimed_id", "https://openid.stackexchange.com/user/f5e91123-e5b4-43c5-871f-5f276c75d31a" },
				{ "openid.identity", "https://openid.stackexchange.com/user/f5e91123-e5b4-43c5-871f-5f276c75d31a" },
				{ "openid.mode", "check_authentication" },
				{ "openid.op_endpoint", "https://openid.stackexchange.com/openid/provider" },
				{ "openid.response_nonce", "2011-08-01T00:32:10Zvdyt3efw" },
				{ "openid.return_to", "http://openid-consumer.appspot.com/finish?session_id=1543025&janrain_nonce=2011-08-01T00%3A32%3A09ZIPGz7D" },
				{ "openid.sig", "b0Rll6Kt1KKBWWBEg/qBvW3sQYtmhOUmpI0/UREBVZ0=" },
				{ "openid.signed", "claimed_id,identity,assoc_handle,op_endpoint,return_to,response_nonce,ns.sreg,sreg.email,sreg.fullname" },
				{ "openid.sreg.email", "kevin.montrose@stackoverflow.com" },
				{ "openid.sreg.fullname", "Kevin K Montrose" },
			};
			var message = (CheckAuthenticationRequest)testChannel.Receive(buggyRPMessage, new MessageReceivingEndpoint(OPUri, HttpDeliveryMethods.PostRequest));
			var originalResponse = new IndirectSignedResponse(message, signer.Channel);
			await signer.ProcessIncomingMessageAsync(originalResponse, CancellationToken.None);
		}
	}
}
