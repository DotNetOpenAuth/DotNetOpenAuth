//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.ChannelElements {
	using System;
	using System.Linq;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class SigningBindingElementTests : OpenIdTestBase {
		/// <summary>
		/// Verifies that the signatures generated match Known Good signatures.
		/// </summary>
		[TestCase]
		public void SignaturesMatchKnownGood() {
			Protocol protocol = Protocol.V20;
			var settings = new ProviderSecuritySettings();
			var store = new ProviderAssociationHandleEncoder(new MemoryCryptoKeyStore());
			byte[] associationSecret = Convert.FromBase64String("rsSwv1zPWfjPRQU80hciu8FPDC+GONAMJQ/AvSo1a2M=");
			string handle = store.Serialize(associationSecret, DateTime.UtcNow.AddDays(1), false);
			Association association = HmacShaAssociation.Create(handle, associationSecret, TimeSpan.FromDays(1));
			SigningBindingElement signer = new SigningBindingElement(store, settings);
			signer.Channel = new TestChannel(this.MessageDescriptions);

			IndirectSignedResponse message = new IndirectSignedResponse(protocol.Version, new Uri("http://rp"));
			ITamperResistantOpenIdMessage signedMessage = message;
			message.ProviderEndpoint = new Uri("http://provider");
			signedMessage.UtcCreationDate = DateTime.Parse("1/1/2009");
			signedMessage.AssociationHandle = association.Handle;
			Assert.IsNotNull(signer.ProcessOutgoingMessage(message));
			Assert.AreEqual("o9+uN7qTaUS9v0otbHTuNAtbkpBm14+es9QnNo6IHD4=", signedMessage.Signature);
		}

		/// <summary>
		/// Verifies that all parameters in ExtraData in signed responses are signed.
		/// </summary>
		[TestCase]
		public void SignedResponsesIncludeExtraDataInSignature() {
			Protocol protocol = Protocol.Default;
			SigningBindingElement sbe = new SigningBindingElement(new ProviderAssociationHandleEncoder(new MemoryCryptoKeyStore()), new ProviderSecuritySettings());
			sbe.Channel = new TestChannel(this.MessageDescriptions);
			IndirectSignedResponse response = new IndirectSignedResponse(protocol.Version, RPUri);
			response.ReturnTo = RPUri;
			response.ProviderEndpoint = OPUri;

			response.ExtraData["someunsigned"] = "value";
			response.ExtraData["openid.somesigned"] = "value";

			Assert.IsNotNull(sbe.ProcessOutgoingMessage(response));
			ITamperResistantOpenIdMessage signedResponse = (ITamperResistantOpenIdMessage)response;

			// Make sure that the extra parameters are signed.
			// Since the signing algorithm only allows for signing parameters that start with
			// 'openid.', other parameters should not be signed.
			Assert.IsNotNull(signedResponse.SignedParameterOrder);
			string[] signedParameters = signedResponse.SignedParameterOrder.Split(',');
			Assert.IsTrue(signedParameters.Contains("somesigned"));
			Assert.IsFalse(signedParameters.Contains("someunsigned"));
		}
	}
}
