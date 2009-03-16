//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.ChannelElements {
	using System;
	using System.Linq;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class SigningBindingElementTests : OpenIdTestBase {
		/// <summary>
		/// Verifies that the signatures generated match Known Good signatures.
		/// </summary>
		[TestMethod]
		public void SignaturesMatchKnownGood() {
			Protocol protocol = Protocol.Default;
			var settings = new ProviderSecuritySettings();
			var store = new AssociationMemoryStore<AssociationRelyingPartyType>();
			byte[] associationSecret = Convert.FromBase64String("rsSwv1zPWfjPRQU80hciu8FPDC+GONAMJQ/AvSo1a2M=");
			Association association = HmacShaAssociation.Create("mock", associationSecret, TimeSpan.FromDays(1));
			store.StoreAssociation(AssociationRelyingPartyType.Smart, association);
			SigningBindingElement signer = new SigningBindingElement(store, settings);
			signer.Channel = new TestChannel(this.MessageDescriptions);

			IndirectSignedResponse message = new IndirectSignedResponse(protocol.Version, new Uri("http://rp"));
			ITamperResistantOpenIdMessage signedMessage = message;
			message.ProviderEndpoint = new Uri("http://provider");
			signedMessage.UtcCreationDate = DateTime.Parse("1/1/2009");
			signedMessage.AssociationHandle = association.Handle;
			Assert.IsNotNull(signer.ProcessOutgoingMessage(message));
			Assert.AreEqual("0wOdvNgzCZ5I5AzbU58Nq2Tg8EJZ7QoNz4gpx2r7jII=", signedMessage.Signature);
		}

		/// <summary>
		/// Verifies that all parameters in ExtraData in signed responses are signed.
		/// </summary>
		[TestMethod]
		public void SignedResponsesIncludeExtraDataInSignature() {
			Protocol protocol = Protocol.Default;
			SigningBindingElement sbe = new SigningBindingElement(new AssociationMemoryStore<AssociationRelyingPartyType>(), new ProviderSecuritySettings());
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
